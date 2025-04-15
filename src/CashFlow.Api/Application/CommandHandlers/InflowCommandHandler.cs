using CashFlow.Api.Application.Commands;
using CashFlow.Api.Domain.Aggregates.CashFlow;
using CashFlow.Api.Domain.Events;
using CashFlow.Api.Infrastructure.Messaging;
using CashFlow.Api.Infrastructure.Persistence.NoSql.Interfaces;
using MediatR;
using MongoDB.Bson.Serialization;

namespace CashFlow.Api.Application.CommandHandlers;

public class InflowCommandHandler : IRequestHandler<InflowCommand, Guid>
{
    private readonly IEventStore _eventStore;
    private readonly ISnapshotStore _snapshotStore;
    private readonly Publisher<InflowProcessedEvent> _publisher;
    private readonly ILogger<InflowCommandHandler> _logger;

    public InflowCommandHandler(IEventStore eventStore, ISnapshotStore snapshotStore, IPublisherFactory publisherFactory, ILogger<InflowCommandHandler> logger)
    {
        _eventStore = eventStore;
        _snapshotStore = snapshotStore;
        _logger = logger;

        _publisher = publisherFactory.CreatePublisher<InflowProcessedEvent>();
    }

    public async Task<Guid> Handle(InflowCommand request, CancellationToken cancellationToken)
    {
        var aggregateId = $"{request.BankAccountId}_{DateTime.UtcNow:yyyy-MM-dd}";
        var snapshot = await _snapshotStore.GetSnapshotAsync(aggregateId);

        CashFlowAggregateRoot aggregate;
        if (snapshot != null)
        {
            aggregate = BsonSerializer.Deserialize<CashFlowAggregateRoot>(snapshot.AggregateData);
        }
        else
        {
            snapshot ??= await _snapshotStore.GetLastSnapshotAsync(request.BankAccountId);
            if (snapshot != null)
            {
                aggregate = new CashFlowAggregateRoot(aggregateId, DateTime.UtcNow.Date)
                {
                    BankAccountId = snapshot.BankAccountId,
                    BalanceStartDay = snapshot.BalanceEnd,
                    BalanceEndDay = snapshot.BalanceEnd
                };
            }
            else
            {
                aggregate = new CashFlowAggregateRoot(aggregateId, DateTime.UtcNow.Date)
                {
                    BankAccountId = request.BankAccountId,
                    BalanceStartDay = 0,
                    BalanceEndDay = 0
                };
            }
        }

        var eventDate = DateTime.UtcNow;
        Guid orderTransactionId = aggregate.ApplyInflow(request.Amount, request.Description, eventDate);

        aggregate.NotifyInflowProcessed(orderTransactionId, request.Amount, request.Description, eventDate);

        var @events = aggregate.GetUncommittedEvents();

        await _eventStore.AppendEventsAsync(aggregate.AggregateId, @events);
        await _snapshotStore.SaveSnapshotAsync(aggregate);

        foreach (var @event in @events.Where(p => typeof(InflowRequestedEvent) != p.GetType()))
        {
            await _publisher.PublishAsync((InflowProcessedEvent)@event);
        }

        return await Task.FromResult(orderTransactionId);
    }
}
