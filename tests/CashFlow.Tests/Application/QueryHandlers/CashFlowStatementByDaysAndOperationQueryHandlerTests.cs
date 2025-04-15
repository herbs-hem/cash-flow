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
using NSubstitute.ClearExtensions;

namespace CashFlow.Tests.Application.QueryHandlers;

public class CashFlowStatementByDaysAndOperationQueryHandlerTests : CashFlowQueryHandlerBaseTests
{
    private readonly Fixture _fixture;

    public CashFlowStatementByDaysAndOperationQueryHandlerTests()
    {
        _fixture = new();
    }

    [Fact]
    public async Task DaysAndOperationQueryHandler_Handle_Should_Return_Statement_FromCache_When_CacheExists()
    {
        // Arrange
        var query =
            _fixture.Build<CashFlowStatementByDaysAndOperationQuery>()
                    .With(x => x.CompanyAccountId, CompanyAccountId)
                    .With(x => x.Days, StatementRulesConstant.MaximumDaysLimit - 1)
                    .With(x => x.OperationType, OperationType.Inflow)
                    .Create();
        var expectedStatement = new CashFlowStatementReadModel(query.CompanyAccountId, new List<StatementDetailsReadModel>(), 5);

        var service = Substitute.For<ICashFlowTransactionService>();
        var cacheClient = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowStatementByDaysAndOperationQueryHandler>>();

        service.CompanyAccountExistsAsync(query.CompanyAccountId).ReturnsForAnyArgs(true);

        cacheClient.TryGetValue(Arg.Any<string>(), out CashFlowStatementReadModel bankStatement)
            .Returns(x =>
            {
                x[1] = expectedStatement;
                return true;
            });

        var handler = new CashFlowStatementByDaysAndOperationQueryHandler(service, cacheClient, logger);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedStatement);
        await service.DidNotReceive().GetStatementAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<OperationType>());
    }

    [Fact]
    public async Task DaysAndOperationQueryHandler_Handle_Should_Return_Statement_FromService_When_CacheMiss()
    {
        // Arrange
        var query =
            _fixture.Build<CashFlowStatementByDaysAndOperationQuery>()
                    .With(x => x.CompanyAccountId, CompanyAccountId)
                    .With(x => x.Days, StatementRulesConstant.MinimumDaysLimit + 1)
                    .With(x => x.OperationType, OperationType.Outflow)
                    .Create();
        var expectedStatement = new CashFlowStatementReadModel(query.CompanyAccountId, new List<StatementDetailsReadModel>(), 2);

        var service = Substitute.For<ICashFlowTransactionService>();
        var cacheClient = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowStatementByDaysAndOperationQueryHandler>>();

        service.CompanyAccountExistsAsync(query.CompanyAccountId).ReturnsForAnyArgs(true);
        service.GetStatementAsync(query.CompanyAccountId, query.Days, query.OperationType)
           .Returns(expectedStatement);

        cacheClient.TryGetValue(Arg.Any<string>(), out CashFlowStatementReadModel bankStatement)
            .Returns(x =>
            {
                x[1] = null;
                return false;
            });

        var handler = new CashFlowStatementByDaysAndOperationQueryHandler(service, cacheClient, logger);
        
        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedStatement);
        await cacheClient.Received().SetAsync(Arg.Any<string>(), expectedStatement);
    }

    [Fact()]
    public async Task DaysAndOperationQueryHandler_Handle_Should_Return_EmptyStatement_When_Service_ReturnsNull()
    {
        // Arrange
        var query =
            _fixture.Build<CashFlowStatementByDaysAndOperationQuery>()
                    .With(x => x.CompanyAccountId, CompanyAccountId)
                    .With(x => x.Days, StatementRulesConstant.MinimumDaysLimit + 1)
                    .With(x => x.OperationType, OperationType.Inflow)
                    .Create();


        var service = Substitute.For<ICashFlowTransactionService>();
        var cacheClient = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowStatementByDaysAndOperationQueryHandler>>();

        service.CompanyAccountExistsAsync(query.CompanyAccountId).Returns(true);
        service.GetStatementAsync(query.CompanyAccountId, query.Days, query.OperationType)
            .Returns((CashFlowStatementReadModel?)null);

        cacheClient.TryGetValue(Arg.Any<string>(), out CashFlowStatementReadModel bankStatement)
            .Returns(x =>
            {
                x[1] = null;
                return false;
            });

        var handler = new CashFlowStatementByDaysAndOperationQueryHandler(service, cacheClient, logger);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CompanyAccountId.Should().Be(query.CompanyAccountId);
        result.Value.Statements.Should().BeEmpty();
        result.Value.CurrentBalance.Should().Be(0);
    }

    [Fact]
    public async Task DaysAndOperationQueryHandler_Handle_Should_Return_Failure_When_Request_IsNull()
    {
        // Arrange
        CashFlowStatementByDaysAndOperationQuery query = null;

        var service = Substitute.For<ICashFlowTransactionService>();
        var cacheClient = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowStatementByDaysAndOperationQueryHandler>>();
        var handler = new CashFlowStatementByDaysAndOperationQueryHandler(service, cacheClient, logger);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CashFlowStatementErrors.RequiredRequest);
    }

    [Fact]
    public async Task DaysAndOperationQueryHandler_Handle_Should_Return_Failure_When_Request_InvalidCompanyAccountId()
    {
        // Arrange
        var query = _fixture.Build<CashFlowStatementByDaysAndOperationQuery>()
            .With(x => x.CompanyAccountId, Guid.Empty)
            .Create();
        
        var service = Substitute.For<ICashFlowTransactionService>();
        var cacheClient = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowStatementByDaysAndOperationQueryHandler>>();
        var handler = new CashFlowStatementByDaysAndOperationQueryHandler(service, cacheClient, logger);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CashFlowStatementErrors.WithInvalidCompanyAccountId(query.CompanyAccountId));
    }

    [Fact]
    public async Task DaysAndOperationQueryHandler_Handle_Should_Return_Failure_When_Request_DaysIsOutOfRange()
    {
        // Arrange
        var expectedError = CashFlowStatementErrors.FailedValidationRules($"Days: Days outside the allowed range ({StatementRulesConstant.MinimumDaysLimit} to {StatementRulesConstant.MaximumDaysLimit})");
        var query =
            _fixture.Build<CashFlowStatementByDaysAndOperationQuery>()
                    .With(x => x.CompanyAccountId, CompanyAccountId)
                    .With(x => x.Days, StatementRulesConstant.MinimumDaysLimit - 1)
                    .With(x => x.OperationType, OperationType.Inflow)
                    .Create();

        var service = Substitute.For<ICashFlowTransactionService>();
        var cacheClient = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowStatementByDaysAndOperationQueryHandler>>();

        service.CompanyAccountExistsAsync(query.CompanyAccountId).ReturnsForAnyArgs(true);

        var handler = new CashFlowStatementByDaysAndOperationQueryHandler(service, cacheClient, logger);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().Be(expectedError);
    }

    [Fact]
    public async Task DaysAndOperationQueryHandler_Handle_Should_Return_Failure_When_Request_OperationType_IsAll()
    {
        // Arrange
        var query = _fixture.Build<CashFlowStatementByDaysAndOperationQuery>()
            .With(x => x.CompanyAccountId, CompanyAccountId)
            .With(x => x.Days, StatementRulesConstant.MinimumDaysLimit + 1)
            .With(x => x.OperationType, OperationType.All)
            .Create();

        var service = Substitute.For<ICashFlowTransactionService>();
        var cacheClient = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowStatementByDaysAndOperationQueryHandler>>();

        service.CompanyAccountExistsAsync(query.CompanyAccountId).ReturnsForAnyArgs(true);

        var handler = new CashFlowStatementByDaysAndOperationQueryHandler(service, cacheClient, logger);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CashFlowStatementErrors.FailedValidationRules("OperationType: Query only allows Inflow or Outflow transactions!"));
    }
}