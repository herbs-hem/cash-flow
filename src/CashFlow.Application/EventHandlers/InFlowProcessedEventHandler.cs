using CashFlow.Application.Commands;
using CashFlow.Application.Services;
using Microsoft.Extensions.Logging;
using CashFlow.Domain.Events;
using MassTransit.Mediator;

namespace CashFlow.Application.EventHandlers;

public class InFlowProcessedEventHandler : IEventHandler<InFlowProcessedEvent, Guid>
{
    private readonly ITransactionsService _service;
    private readonly IMediator _mediator;
    private readonly ILogger<InFlowProcessedEventHandler> _logger;

    public InFlowProcessedEventHandler(ITransactionsService service, IMediator mediator, ILogger<InFlowProcessedEventHandler> logger)
    {
        _service = service;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Guid> HandleAsync(InFlowProcessedEvent @event)
    {
        _logger.LogDebug("Processed credit transaction: {EntityIdentifier} {EventData}", 
            @event.CompanyAccountId, 
            System.Text.Json.JsonSerializer.Serialize(@event));

        var transactionId = await _service.InFlowAsync(
            @event.CompanyAccountId, 
            @event.Amount, 
            @event.Description, 
            @event.OccurredAt, 
            @event.BalanceStartDay, 
            @event.BalanceEndDay);

        // Emite o comando de forma assíncrona (não espera a conclusão)
        await _mediator.Send(new ReprocessConsolidatedReportsCommand(
            @event.CompanyAccountId,
            @event.OccurredAt));

        return transactionId;
    }
}