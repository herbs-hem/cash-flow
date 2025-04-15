using CashFlow.Api.Domain.Events;
using CashFlow.Api.Domain.Services;

namespace CashFlow.Api.Application.EventHandlers;

public class InflowProcessedEventHandler : IEventHandler<InflowProcessedEvent, Guid>
{
    private readonly ICashFlowTransactionService _service;
    private readonly ILogger<InflowProcessedEventHandler> _logger;

    public InflowProcessedEventHandler(ICashFlowTransactionService service, ILogger<InflowProcessedEventHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Guid> HandleAsync(InflowProcessedEvent @event)
    {
        _logger.LogDebug($"Processed credit transaction: {{CompanyAccountId}} {Environment.NewLine} {{EventData}}", @event.CompanyAccountId, System.Text.Json.JsonSerializer.Serialize(@event));

        return await _service.CreditAsync(@event.CompanyAccountId, @event.Amount, @event.Description, @event.OccurredAt, @event.BalanceStartDay, @event.BalanceEndDay);
    }
}