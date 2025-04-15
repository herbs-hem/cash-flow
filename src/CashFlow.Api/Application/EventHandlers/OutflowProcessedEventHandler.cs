using CashFlow.Api.Domain.Events;
using CashFlow.Api.Domain.Services;

namespace CashFlow.Api.Application.EventHandlers;

public class OutflowProcessedEventHandler : IEventHandler<OutflowProcessedEvent, Guid>
{
    private readonly ICashFlowTransactionService _service;
    private readonly ILogger<OutflowProcessedEventHandler> _logger;

    public OutflowProcessedEventHandler(ICashFlowTransactionService service, ILogger<OutflowProcessedEventHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Guid> HandleAsync(OutflowProcessedEvent @event)
    {
        _logger.LogInformation($"Pedido criado: {{CompanyAccountId}} {Environment.NewLine} {{EventData}}", @event.CompanyAccountId, System.Text.Json.JsonSerializer.Serialize(@event));

        return await _service.DebitAsync(@event.CompanyAccountId, @event.Amount, @event.Description, @event.OccurredAt, @event.BalanceStartDay, @event.BalanceEndDay);
    }
}