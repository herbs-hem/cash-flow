using AutoFixture;
using CashFlow.Api.Application.EventHandlers;
using CashFlow.Api.Domain.Events;
using CashFlow.Api.Domain.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CashFlow.Tests.Application.EventHandlers;

public class OutflowProcessedEventHandlerTests
{
    private readonly IFixture _fixture;
    private readonly ICashFlowTransactionService _service;
    private readonly ILogger<OutflowProcessedEventHandler> _logger;
    private readonly OutflowProcessedEventHandler _handler;

    public OutflowProcessedEventHandlerTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task HandleAsync_Should_Call_OutflowAsync_And_Return_TransactionId()
    {
        // Arrange
        var debitEvent = _fixture.Create<OutflowProcessedEvent>();
        var expectedTransactionId = Guid.NewGuid();

        var service = Substitute.For<ICashFlowTransactionService>();
        var logger = Substitute.For<ILogger<OutflowProcessedEventHandler>>();

        service.OutflowAsync(
            debitEvent.CompanyAccountId,
            debitEvent.Amount,
            debitEvent.Description,
            debitEvent.OccurredAt,
            debitEvent.BalanceStartDay,
            debitEvent.BalanceEndDay)
            .Returns(expectedTransactionId);

        var handler = new OutflowProcessedEventHandler(service, logger);

        // Act
        var result = await handler.HandleAsync(debitEvent);

        // Assert
        await service.Received(1).OutflowAsync(
            debitEvent.CompanyAccountId,
            debitEvent.Amount,
            debitEvent.Description,
            debitEvent.OccurredAt,
            debitEvent.BalanceStartDay,
            debitEvent.BalanceEndDay);

        result.Should().Be(expectedTransactionId);

        logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Pedido criado")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }
}
