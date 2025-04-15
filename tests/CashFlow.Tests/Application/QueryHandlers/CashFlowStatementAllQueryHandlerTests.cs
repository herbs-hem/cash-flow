using AutoFixture;
using CashFlow.Api.Application.Queries.Statement;
using CashFlow.Api.Application.QueryHandlers.Statement;
using CashFlow.Api.Domain.Constants;
using CashFlow.Api.Domain.Enums;
using CashFlow.Api.Domain.Services;
using CashFlow.Api.Infrastructure.Cache.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CashFlow.Tests.Application.QueryHandlers;

public class CashFlowStatementAllQueryHandlerTests : CashFlowQueryHandlerBaseTests
{
    private readonly IFixture _fixture;

    public CashFlowStatementAllQueryHandlerTests()
    {
        _fixture = new Fixture();       
    }

    [Fact]
    public async Task AllQueryHandler_Handle_Should_Return_Statement_FromCache_When_CacheExists()
    {
        // Arrange
        var query = 
            _fixture.Build<CashFlowStatementAllQuery>()
                    .With(x => x.CompanyAccountId, CompanyAccountId)
                    .Create();
        var expectedStatement = new CashFlowStatementReadModel(query.CompanyAccountId, new List<StatementDetailsReadModel>(), 5);

        var service = Substitute.For<ICashFlowTransactionService>();
        var cacheClient = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowStatementAllQueryHandler>>();

        service.CompanyAccountExistsAsync(query.CompanyAccountId).ReturnsForAnyArgs(true);

        cacheClient.TryGetValue(Arg.Any<string>(), out CashFlowStatementReadModel bankStatement)
            .Returns(x =>
            {
                x[1] = expectedStatement;
                return true;
            });

        var handler = new CashFlowStatementAllQueryHandler(service, cacheClient, logger);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedStatement);
        await service.DidNotReceive().GetStatementAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<OperationType>());
    }

    [Fact]
    public async Task AllQueryHandler_Handle_Should_Return_Statement_FromService_When_CacheMiss()
    {
        // Arrange
        var query =
            _fixture.Build<CashFlowStatementAllQuery>()
                    .With(x => x.CompanyAccountId, CompanyAccountId)
                    .Create();
        var expectedStatement = new CashFlowStatementReadModel(query.CompanyAccountId, new List<StatementDetailsReadModel>(), 2);

        var service = Substitute.For<ICashFlowTransactionService>();
        var cacheClient = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowStatementAllQueryHandler>>();

        service.CompanyAccountExistsAsync(query.CompanyAccountId).ReturnsForAnyArgs(true);
        service.GetStatementAsync(query.CompanyAccountId, StatementRulesConstant.MinimumDaysLimit, OperationType.All)
           .Returns(expectedStatement);

        cacheClient.TryGetValue(Arg.Any<string>(), out CashFlowStatementReadModel bankStatement)
            .Returns(x =>
            {
                x[1] = null;
                return false;
            });

        var handler = new CashFlowStatementAllQueryHandler(service, cacheClient, logger);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedStatement);
        await cacheClient.Received().SetAsync(Arg.Any<string>(), expectedStatement);
    }

    [Fact]
    public async Task AllQueryHandler_Handle_Should_Return_EmptyStatement_When_Service_ReturnsNull()
    {
        // Arrange
        var query =
            _fixture.Build<CashFlowStatementAllQuery>()
                    .With(x => x.CompanyAccountId, CompanyAccountId)
                    .Create();

        var service = Substitute.For<ICashFlowTransactionService>();
        var cacheClient = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowStatementAllQueryHandler>>();

        service.CompanyAccountExistsAsync(query.CompanyAccountId).ReturnsForAnyArgs(true);
        service.GetStatementAsync(query.CompanyAccountId, StatementRulesConstant.MinimumDaysLimit, OperationType.All)
           .Returns((CashFlowStatementReadModel?)null);

        cacheClient.TryGetValue(Arg.Any<string>(), out CashFlowStatementReadModel bankStatement)
            .Returns(x =>
            {
                x[1] = null;
                return false;
            });

        var handler = new CashFlowStatementAllQueryHandler(service, cacheClient, logger);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CompanyAccountId.Should().Be(query.CompanyAccountId);
        result.Value.Statements.Should().BeEmpty();
        result.Value.CurrentBalance.Should().Be(0);
    }

    [Fact]
    public async Task AllQueryHandler_Handle_Should_Return_Failure_When_Request_IsNull()
    {
        // Arrange
        CashFlowStatementAllQuery query = null;

        var service = Substitute.For<ICashFlowTransactionService>();
        var cacheClient = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowStatementAllQueryHandler>>();
        var handler = new CashFlowStatementAllQueryHandler(service, cacheClient, logger);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CashFlowStatementErrors.RequiredRequest);
    }

    [Fact]
    public async Task AllQueryHandler_Handle_Should_Return_Failure_When_Request_InvalidCompanyAccountId()
    {
        // Arrange
        var query =
            _fixture.Build<CashFlowStatementAllQuery>()
                    .With(x => x.CompanyAccountId, Guid.Empty)
                    .Create();

        var service = Substitute.For<ICashFlowTransactionService>();
        var cacheClient = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowStatementAllQueryHandler>>();
        var handler = new CashFlowStatementAllQueryHandler(service, cacheClient, logger);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CashFlowStatementErrors.WithInvalidCompanyAccountId(query.CompanyAccountId));
    }

    [Fact]
    public async Task AllQueryHandler_Handle_Should_Return_Failure_When_Request_CompanyAccountId_NotFound()
    {
        // Arrange
        var query =
            _fixture.Build<CashFlowStatementAllQuery>()
                    .With(x => x.CompanyAccountId, CompanyAccountId)
                    .Create();

        var service = Substitute.For<ICashFlowTransactionService>();
        var cacheClient = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowStatementAllQueryHandler>>();

        cacheClient.TryGetValue(Arg.Any<string>(), out CashFlowStatementReadModel bankStatement)
            .Returns(x =>
            {
                x[1] = null;
                return false;
            });

        service.CompanyAccountExistsAsync(query.CompanyAccountId).ReturnsForAnyArgs(false);

        var handler = new CashFlowStatementAllQueryHandler(service, cacheClient, logger);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CashFlowStatementErrors.FailedValidationRules($"{nameof(query.CompanyAccountId)}: {query.CompanyAccountId} not found!"));
    }
}