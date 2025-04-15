using CashFlow.Api.Application.Commands;
using CashFlow.Api.Domain.Aggregates.CashFlow;
using CashFlow.Api.Domain.Events;
using CashFlow.Api.Infrastructure.Messaging;
using CashFlow.Api.Infrastructure.Persistence.NoSql.Interfaces;
using MediatR;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json;

namespace CashFlow.Api.Application.CommandHandlers
{
    public class OutflowCommandHandler : IRequestHandler<OutflowCommand, Guid>
    {
        private readonly IEventStore _eventStore;
        private readonly ISnapshotStore _snapshotStore;
        private readonly ILogger<OutflowCommandHandler> _logger;
        private readonly Publisher<OutflowProcessedEvent> _publisher;

        public OutflowCommandHandler(
            IEventStore eventStore, ISnapshotStore snapshotStore, 
            IPublisherFactory publisherFactory, ILogger<OutflowCommandHandler> logger)
        {
            _eventStore = eventStore;
            _snapshotStore = snapshotStore;
            _logger = logger;
            _publisher = publisherFactory.CreatePublisher<OutflowProcessedEvent>();
        }

        public async Task<Guid> Handle(OutflowCommand request, CancellationToken cancellationToken)
        {
            var aggregateId = $"{request.CompanyAccountId}_{DateTime.UtcNow:yyyy-MM-dd}";
            var snapshot = await _snapshotStore.GetSnapshotAsync(aggregateId);

            CashFlowAggregateRoot aggregate;
            if (snapshot != null)
            {
                aggregate = BsonSerializer.Deserialize<CashFlowAggregateRoot>(snapshot.AggregateData)!;
                _logger.LogDebug("AggregateSnapshot found! Snapshot: {AggregateSnapshot}", JsonConvert.SerializeObject(aggregate));
            }
            else
            {
                snapshot ??= await _snapshotStore.GetLastSnapshotAsync(request.CompanyAccountId);
                if (snapshot != null)
                {
                    aggregate = new CashFlowAggregateRoot(aggregateId, DateTime.UtcNow.Date)
                    {
                        CompanyAccountId = snapshot.CompanyAccountId,
                        BalanceStartDay = snapshot.BalanceEnd,
                        BalanceEndDay = snapshot.BalanceEnd
                    };
                    _logger.LogDebug("Last AggregateSnapshot found! Snapshot: {AggregateSnapshot}", JsonConvert.SerializeObject(aggregate));
                }
                else
                {
                    aggregate = new CashFlowAggregateRoot(aggregateId, DateTime.UtcNow.Date)
                    {
                        CompanyAccountId = request.CompanyAccountId,
                        BalanceStartDay = 0,
                        BalanceEndDay = 0
                    };
                    _logger.LogDebug("AggregateSnapshot not found!New one has been created! Snapshot: {AggregateSnapshot}", JsonConvert.SerializeObject(aggregate));
                }
            }

            var eventDate = DateTime.UtcNow;
            Guid orderTransactionId = aggregate.ApplyOutflow(request.Amount, request.Description, eventDate);
            aggregate.NotifyOutflowProcessed(orderTransactionId, request.Amount, request.Description, eventDate);

            var @events = aggregate.GetUncommittedEvents();

            await _eventStore.AppendEventsAsync(aggregate.AggregateId, @events);
            await _snapshotStore.SaveSnapshotAsync(aggregate);

            foreach (var @event in @events.Where(p => typeof(OutflowRequestedEvent) != p.GetType()))
            {
                await _publisher.PublishAsync((OutflowProcessedEvent)@event);
                _logger.LogDebug("Published event! EventName: {EventName} - EventData: {EventData}", nameof(OutflowProcessedEvent), JsonConvert.SerializeObject(@event, type: typeof(OutflowProcessedEvent), null));
            }

            return await Task.FromResult(orderTransactionId);
        }
    }
}
