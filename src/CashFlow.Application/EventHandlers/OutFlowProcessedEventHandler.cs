using CashFlow.Application.Services;
using Microsoft.Extensions.Logging;
using CashFlow.Domain.Events;

namespace CashFlow.Application.EventHandlers;

public class OutFlowProcessedEventHandler : IEventHandler<OutFlowProcessedEvent, Guid>
{
    private readonly ITransactionsService _service;
    private readonly ILogger<OutFlowProcessedEventHandler> _logger;

    public OutFlowProcessedEventHandler(ITransactionsService service, ILogger<OutFlowProcessedEventHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Guid> HandleAsync(OutFlowProcessedEvent @event)
    {
        _logger.LogInformation($"Pedido criado: {{EntityIdentifier}} {Environment.NewLine} {{EventData}}", @event.CompanyAccountId, System.Text.Json.JsonSerializer.Serialize(@event));

        return await _service.OutFlowAsync(@event.CompanyAccountId, @event.Amount, @event.Description, @event.OccurredAt, @event.BalanceStartDay, @event.BalanceEndDay);
    }
}