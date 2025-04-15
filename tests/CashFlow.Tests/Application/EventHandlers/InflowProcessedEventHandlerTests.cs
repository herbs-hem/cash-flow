using AutoFixture;
using CashFlow.Api.Application.EventHandlers;
using CashFlow.Api.Domain.Events;
using CashFlow.Api.Domain.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CashFlow.Tests.Application.EventHandlers;

public class InflowProcessedEventHandlerTests
{
    private readonly IFixture _fixture;

    public InflowProcessedEventHandlerTests()
    {
        _fixture = new Fixture();       
    }

    [Fact]
    public async Task HandleAsync_Should_Call_InflowAsync_And_Return_TransactionId()
    {
        // Arrange
        var creditEvent = _fixture.Create<InflowProcessedEvent>();
        var expectedTransactionId = Guid.NewGuid();

        var service = Substitute.For<ICashFlowTransactionService>();
        var logger = Substitute.For<ILogger<InflowProcessedEventHandler>>();

        service.InflowAsync(
            creditEvent.CompanyAccountId,
            creditEvent.Amount,
            creditEvent.Description,
            creditEvent.OccurredAt,
            creditEvent.BalanceStartDay,
            creditEvent.BalanceEndDay)
            .Returns(expectedTransactionId);

        var handler = new InflowProcessedEventHandler(service, logger);

        // Act
        var result = await handler.HandleAsync(creditEvent);

        // Assert
        await service.Received(1).InflowAsync(
            creditEvent.CompanyAccountId,
            creditEvent.Amount,
            creditEvent.Description,
            creditEvent.OccurredAt,
            creditEvent.BalanceStartDay,
            creditEvent.BalanceEndDay);

        result.Should().Be(expectedTransactionId);

        logger.Received().Log(
            LogLevel.Debug,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Processed credit transaction")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }
}
