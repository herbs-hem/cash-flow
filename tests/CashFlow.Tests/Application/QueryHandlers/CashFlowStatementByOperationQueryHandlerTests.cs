
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using CashFlow.Api.Application.Queries.Statement;
using CashFlow.Api.Application.QueryHandlers.Statement;
using CashFlow.Api.Domain.Constants;
using CashFlow.Api.Domain.Enums;
using CashFlow.Api.Domain.Services;
using CashFlow.Api.Infrastructure.Cache.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Extensions;
using NSubstitute;
using NSubstitute.Core;
using System.Reflection.Metadata;

namespace CashFlow.Tests.Application.QueryHandlers;

public class CashFlowStatementByOperationQueryHandlerTests : CashFlowQueryHandlerBaseTests
{
    private readonly Fixture _fixture;

    public CashFlowStatementByOperationQueryHandlerTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_ValidationFails()
    {
        // Arrange
        var service = Substitute.For<ICashFlowTransactionService>();
        var cache = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowStatementByOperationQueryHandler>>();

        var handler = new CashFlowStatementByOperationQueryHandler(service, cache, logger);

        var invalidRequest = (CashFlowStatementByOperationQuery)null!;

        // Act
        var result = await handler.Handle(invalidRequest, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CashFlowStatementErrors.RequiredRequest);
    }

    [Fact]
    public async Task Handle_Should_ReturnStatement_FromCache_When_CacheExists()
    {
        // Arrange
        var service = Substitute.For<ICashFlowTransactionService>();
        var cache = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowStatementByOperationQueryHandler>>();

        var request = _fixture.Build<CashFlowStatementByOperationQuery>()
            .With(x => x.CompanyAccountId, Guid.NewGuid())
            .With(x => x.OperationType, OperationType.Inflow)
            .Create();

        var statement = new CashFlowStatementReadModel(request.CompanyAccountId, new List<StatementDetailsReadModel>(), 100);
        var key = $"{request.OperationType.GetDisplayName()}StatementQuery_{request.CompanyAccountId}_{DateTime.UtcNow.Date:yyyyMMdd}";

        cache.TryGetValue(key, out Arg.Any<CashFlowStatementReadModel>())
             .Returns(x => {
                 x[1] = statement;
                 return true;
             });

        service.CompanyAccountExistsAsync(request.CompanyAccountId).Returns(true);

        var handler = new CashFlowStatementByOperationQueryHandler(service, cache, logger);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(statement);
    }

    [Fact]
    public async Task Handle_Should_Return_Statement_FromService_When_CacheMiss()
    {
        // Arrange
        var fixture = _fixture.Customize(new AutoNSubstituteCustomization());
        var request = 
            _fixture.Build<CashFlowStatementByOperationQuery>()
                .With(x => x.CompanyAccountId, CompanyAccountId)
                .With(x => x.OperationType, OperationType.Outflow)
                .Create();

        var expected = new CashFlowStatementReadModel(request.CompanyAccountId, Statements: [], CurrentBalance: 200);

        var service = fixture.Freeze<ICashFlowTransactionService>();
        var cache = fixture.Freeze<ICacheClient>();
        var logger = fixture.Freeze<ILogger<CashFlowStatementByOperationQueryHandler>>();

        service.CompanyAccountExistsAsync(request.CompanyAccountId).Returns(true);

        var cacheKey = $"{request.OperationType.GetDisplayName()}StatementQuery_{request.CompanyAccountId}_{DateTime.UtcNow.Date:yyyyMMdd}";
        cache.TryGetValue(cacheKey, out Arg.Any<CashFlowStatementReadModel>())
             .Returns(x => {
                 x[1] = null;
                 return false;
             });

        service.GetStatementAsync(request.CompanyAccountId, StatementRulesConstant.MinimumDaysLimit, request.OperationType)
               .Returns(expected);

        var handler = new CashFlowStatementByOperationQueryHandler(service, cache, logger);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expected);

        await cache.Received(1).SetAsync(cacheKey, expected);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmptyStatement_When_Service_ReturnsNull()
    {
        // Arrange
        var fixture = _fixture.Customize(new AutoNSubstituteCustomization());
        var query = 
            _fixture.Build<CashFlowStatementByOperationQuery>()
                    .With(x => x.CompanyAccountId, CompanyAccountId)
                    .With(x => x.OperationType, OperationType.Inflow)
                    .Create();

        var service = fixture.Freeze<ICashFlowTransactionService>();
        var cache = fixture.Freeze<ICacheClient>();
        var logger = fixture.Freeze<ILogger<CashFlowStatementByOperationQueryHandler>>();
        var handler = new CashFlowStatementByOperationQueryHandler(service, cache, logger);

        var cacheKey = $"{query.OperationType.GetDisplayName()}StatementQuery_{query.CompanyAccountId}_{DateTime.UtcNow.Date:yyyyMMdd}";
        cache.TryGetValue(cacheKey, out Arg.Any<CashFlowStatementReadModel>())
             .Returns(x => {
                 x[1] = null;
                 return false;
             });

        service.CompanyAccountExistsAsync(query.CompanyAccountId).Returns(true);
        service.GetStatementAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<OperationType>())
               .Returns((CashFlowStatementReadModel?)null);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CompanyAccountId.Should().Be(query.CompanyAccountId);
        result.Value.Statements.Should().BeEmpty();
        result.Value.CurrentBalance.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_RequestIsNull()
    {
        // Arrange
        var service = Substitute.For<ICashFlowTransactionService>();
        var cache = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowStatementByOperationQueryHandler>>();

        var handler = new CashFlowStatementByOperationQueryHandler(service, cache, logger);

        // Act
        var result = await handler.Handle(null!, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CashFlowStatementErrors.RequiredRequest);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_CompanyAccountIdIsEmpty()
    {
        // Arrange
        var query =
            _fixture.Build<CashFlowStatementByOperationQuery>()
                    .With(x => x.CompanyAccountId, Guid.Empty)
                    .With(x => x.OperationType, OperationType.Inflow)
                    .Create();

        var service = Substitute.For<ICashFlowTransactionService>();
        var cache = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowStatementByOperationQueryHandler>>();

        var handler = new CashFlowStatementByOperationQueryHandler(service, cache, logger);

        // Act
        var result = await handler.ValidateAsync(query);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CashFlowStatementErrors.WithInvalidCompanyAccountId(Guid.Empty));
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_CompanyAccountId_NotFound()
    {
        // Arrange
        var query = _fixture.Build<CashFlowStatementByOperationQuery>()
            .With(x => x.CompanyAccountId, Guid.NewGuid())
            .With(x => x.OperationType, OperationType.Inflow)
            .Create();

        var service = Substitute.For<ICashFlowTransactionService>();
        var cache = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowStatementByOperationQueryHandler>>();

        service.CompanyAccountExistsAsync(query.CompanyAccountId).Returns(false);

        var handler = new CashFlowStatementByOperationQueryHandler(service, cache, logger);

        // Act
        var result = await handler.ValidateAsync(query);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CashFlowStatementErrors.FailedValidationRules($"{nameof(query.CompanyAccountId)}: {query.CompanyAccountId} not found!"));
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_OperationTypeIsAll()
    {
        // Arrange
        var query = 
            _fixture.Build<CashFlowStatementByOperationQuery>()
                    .With(x => x.CompanyAccountId, Guid.NewGuid())
                    .With(x => x.OperationType, OperationType.All)
                    .Create();

        var service = Substitute.For<ICashFlowTransactionService>();
        var cache = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowStatementByOperationQueryHandler>>();

        service.CompanyAccountExistsAsync(query.CompanyAccountId).Returns(true);

        var handler = new CashFlowStatementByOperationQueryHandler(service, cache, logger);

        // Act
        var result = await handler.ValidateAsync(query);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CashFlowStatementErrors.FailedValidationRules("OperationType: Query only allows Inflow or Outflow transactions!"));
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_RequestIsValid()
    {
        // Arrange
        var query = 
            _fixture.Build<CashFlowStatementByOperationQuery>()
                    .With(x => x.CompanyAccountId, Guid.NewGuid())
                    .With(x => x.OperationType, OperationType.Inflow)
                    .Create();

        var service = Substitute.For<ICashFlowTransactionService>();
        var cache = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowStatementByOperationQueryHandler>>();

        service.CompanyAccountExistsAsync(query.CompanyAccountId).Returns(true);

        var handler = new CashFlowStatementByOperationQueryHandler(service, cache, logger);

        // Act
        var result = await handler.ValidateAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
