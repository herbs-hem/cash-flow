using CashFlow.Application.Commands;
using MediatR;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json;
using CashFlow.Domain.Abstractions;
using CashFlow.Domain.Aggregates.CashFlow;
using CashFlow.Domain.Documents;
using CashFlow.Domain.Events;
using CashFlow.Infrastructure.Messaging;
using CashFlow.Infrastructure.Persistence.NoSql.Interfaces;
using Microsoft.Extensions.Logging;

namespace CashFlow.Application.CommandHandlers
{
    public class OutFlowRequestCommandHandler : IRequestHandler<OutFlowCommand, Guid>
    {
        private readonly IEventStore _eventStore;
        private readonly ISnapshotStore<CashFlowSnapshot, CashFlowAggregateRoot> _snapshotStore;
        private readonly Publisher<OutFlowProcessedEvent> _publisher;
        private readonly ILogger<InFlowRequestCommandHandler> _logger;

        public OutFlowRequestCommandHandler(IEventStore eventStore,
            ISnapshotStore<CashFlowSnapshot, CashFlowAggregateRoot> snapshotStore,
            IPublisherFactory publisherFactory,
            ILogger<InFlowRequestCommandHandler> logger)
        {
            _eventStore = eventStore;
            _snapshotStore = snapshotStore;
            _publisher = publisherFactory.CreatePublisher<OutFlowProcessedEvent>();
            _logger = logger;
        }

        public async Task<Guid> Handle(OutFlowCommand request, CancellationToken cancellationToken)
        {
            Guid transactionId = Guid.Empty;
            
            try
            {
                var aggregateId = $"{request.CompanyAccountId}_{DateTime.UtcNow:yyyy-MM-dd}";
                var snapshot = await _snapshotStore.GetSnapshotAsync(aggregateId);

                CashFlowAggregateRoot aggregate;
                if (snapshot != null)
                {
                    //aggregate = JsonConvert.DeserializeObject<CashFlowAggregateRoot>(snapshot.AggregateData)!;
                    aggregate = BsonSerializer.Deserialize<CashFlowAggregateRoot>(snapshot.AggregateData)!;
                }
                else
                {
                    snapshot ??= await _snapshotStore.GetLastSnapshotAsync(request.CompanyAccountId);
                    if (snapshot != null)
                    {
                        aggregate = new CashFlowAggregateRoot(aggregateId, DateTime.UtcNow.Date)
                        {
                            CompanyAccountId = snapshot.EntityIdentifier,
                            BalanceStartDay = snapshot.BalanceEnd,
                            BalanceEndDay = snapshot.BalanceEnd
                        };
                    }
                    else
                    {
                        aggregate = new CashFlowAggregateRoot(aggregateId, DateTime.UtcNow.Date)
                        {
                            CompanyAccountId = request.CompanyAccountId,
                            BalanceStartDay = 0,
                            BalanceEndDay = 0
                        };
                    }
                }

                var eventDate = DateTime.UtcNow;
                transactionId = await aggregate.RequestDebit(request.Amount, request.Description, eventDate);
                await aggregate.ConsolidateDebit(transactionId, request.Amount, request.Description, eventDate);
            
                var @events = aggregate.GetUncommittedEvents();

                await _eventStore.AppendEventsAsync(aggregate.AggregateId, @events);
                await _snapshotStore.SaveSnapshotAsync(aggregate);
            
                foreach (var @event in @events.Where(p => typeof(OutFlowProcessedEvent) == p.GetType()))
                {
                    await _publisher.PublishAsync((OutFlowProcessedEvent)@event);
                   
                    _logger.LogDebug("Evento {EventName} publicado {EventData}", nameof(OutFlowProcessedEvent), System.Text.Json.JsonSerializer.Serialize((OutFlowProcessedEvent)@event));
                }

                return transactionId;
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao executar débito: {Message}", ex.Message);
                return transactionId;
            }
        }
    }
}