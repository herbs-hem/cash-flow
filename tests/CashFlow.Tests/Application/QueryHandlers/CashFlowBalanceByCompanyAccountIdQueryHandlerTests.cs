using AutoFixture;
using CashFlow.Api.Application.Queries.Balance;
using CashFlow.Api.Application.QueryHandlers.Balance;
using CashFlow.Api.Domain.Services;
using CashFlow.Api.Infrastructure.Cache.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CashFlow.Tests.Application.QueryHandlers;

public class CashFlowBalanceByCompanyAccountIdQueryHandlerTests : CashFlowQueryHandlerBaseTests
{
    private readonly IFixture _fixture;

    public CashFlowBalanceByCompanyAccountIdQueryHandlerTests()
    {
        _fixture = new Fixture();        
    }

    [Fact]
    public async Task BalanceQueryHandler_Handle_Should_Return_Balance_FromCache_When_CacheExists()
    {
        // Arrange
        var query = 
            _fixture.Build<CashFlowBalanceByCompanyAccountIdQuery>()
            .With(x => x.CompanyAccountId, CompanyAccountId)
            .Create();
        var expectedBalance = new CashFlowBalanceReadModel(query.CompanyAccountId, 500);

        var cacheKey = $"BalanceQuery_{query.CompanyAccountId}_{DateTime.UtcNow.Date:yyyyMMdd}";

        var service = Substitute.For<ICashFlowTransactionService>();
        var cacheClient = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowBalanceByCompanyAccountIdQueryHandler>>();

        cacheClient.TryGetValue(Arg.Any<string>(), out CashFlowBalanceReadModel bankBalance)
           .Returns(x =>
           {
               x[1] = expectedBalance; // out value
               return true;
           });

        service.CompanyAccountExistsAsync(query.CompanyAccountId).ReturnsForAnyArgs(true);

        var handler = new CashFlowBalanceByCompanyAccountIdQueryHandler(service, cacheClient, logger);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedBalance);
        await service.DidNotReceive().GetBalanceAsync(Arg.Any<Guid>()); // Não chama serviço se estiver em cache
    }

    [Fact]
    public async Task BalanceQueryHandler_Handle_Should_Return_Balance_FromService_When_CacheMiss()
    {
        // Arrange
        var query =
         _fixture.Build<CashFlowBalanceByCompanyAccountIdQuery>()
                 .With(x => x.CompanyAccountId, CompanyAccountId)
                 .Create();
        var expectedBalance = new CashFlowBalanceReadModel(query.CompanyAccountId, 200);

        var cacheKey = $"BalanceQuery_{query.CompanyAccountId}_{DateTime.UtcNow.Date:yyyyMMdd}";

        var service = Substitute.For<ICashFlowTransactionService>();
        var cacheClient = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowBalanceByCompanyAccountIdQueryHandler>>();

        cacheClient.TryGetValue(Arg.Any<string>(), out CashFlowBalanceReadModel bankBalance)
           .Returns(x =>
           {
               x[1] = null; // out value
               return false;
           });

        service.CompanyAccountExistsAsync(query.CompanyAccountId).ReturnsForAnyArgs(true);
        service.GetBalanceAsync(query.CompanyAccountId).Returns(expectedBalance);

        var handler = new CashFlowBalanceByCompanyAccountIdQueryHandler(service, cacheClient, logger);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedBalance);

        await service.Received(1).GetBalanceAsync(query.CompanyAccountId);
        await cacheClient.Received(1).SetAsync(cacheKey, expectedBalance);
    }

    [Fact]
    public async Task BalanceQueryHandler_Handle_Should_Return_Empty_Balance_When_Service_Returns_Null()
    {
        // Arrange
        var query =
            _fixture.Build<CashFlowBalanceByCompanyAccountIdQuery>()
                    .With(x => x.CompanyAccountId, CompanyAccountId)
                    .Create();

        var service = Substitute.For<ICashFlowTransactionService>();
        var cacheClient = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowBalanceByCompanyAccountIdQueryHandler>>();

        cacheClient.TryGetValue(Arg.Any<string>(), out CashFlowBalanceReadModel bankBalance)
           .Returns(x =>
           {
               x[1] = null;
               return false;
           });

        service.CompanyAccountExistsAsync(query.CompanyAccountId).ReturnsForAnyArgs(true);
        service.GetBalanceAsync(query.CompanyAccountId).Returns((CashFlowBalanceReadModel?)null);

        var handler = new CashFlowBalanceByCompanyAccountIdQueryHandler(service, cacheClient, logger);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CompanyAccountId.Should().Be(query.CompanyAccountId);
        result.Value.Balance.Should().Be(0);
    }

    [Fact]
    public async Task BalanceQueryHandler_Handle_Should_Return_Failure_When_Request_CompanyAccountId_NotFound()
    {
        // Arrange
        var query =
            _fixture.Build<CashFlowBalanceByCompanyAccountIdQuery>()
                    .With(x => x.CompanyAccountId, CompanyAccountId)
                    .Create();

        var service = Substitute.For<ICashFlowTransactionService>();
        var cacheClient = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowBalanceByCompanyAccountIdQueryHandler>>();

        cacheClient.TryGetValue(Arg.Any<string>(), out CashFlowBalanceReadModel bankBalance)
           .Returns(x =>
           {
               x[1] = null;
               return false;
           });

        service.CompanyAccountExistsAsync(query.CompanyAccountId).ReturnsForAnyArgs(false);

        var handler = new CashFlowBalanceByCompanyAccountIdQueryHandler(service, cacheClient, logger);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CashFlowBalanceErrors.FailedValidationRules($"{nameof(query.CompanyAccountId)}: {query.CompanyAccountId} not found!"));
    }

    [Fact]
    public async Task BalanceQueryHandler_Handle_Should_Return_Failure_When_Request_InvalidCompanyAccountId()
    {
        // Arrange
        var query =
            _fixture.Build<CashFlowBalanceByCompanyAccountIdQuery>()
                    .With(x => x.CompanyAccountId, Guid.Empty)
                    .Create();

        var service = Substitute.For<ICashFlowTransactionService>();
        var cacheClient = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowBalanceByCompanyAccountIdQueryHandler>>();
        var handler = new CashFlowBalanceByCompanyAccountIdQueryHandler(service, cacheClient, logger);

        service.CompanyAccountExistsAsync(query.CompanyAccountId).ReturnsForAnyArgs(true);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CashFlowBalanceErrors.WithInvalidCompanyAccountId(query.CompanyAccountId));
    }

    [Fact]
    public async Task BalanceQueryHandler_Handle_Should_Return_Failure_When_Request_IsNull()
    {
        // Arrange
        CashFlowBalanceByCompanyAccountIdQuery? request = null;
        var service = Substitute.For<ICashFlowTransactionService>();
        var cacheClient = Substitute.For<ICacheClient>();
        var logger = Substitute.For<ILogger<CashFlowBalanceByCompanyAccountIdQueryHandler>>();
        var handler = new CashFlowBalanceByCompanyAccountIdQueryHandler(service, cacheClient, logger);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CashFlowBalanceErrors.RequiredRequest);
    }
}
