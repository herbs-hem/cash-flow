using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using CashFlow.Domain.Documents;
using CashFlow.Domain.Events;
using CashFlow.Infrastructure.Messaging;

namespace CashFlow.Application.Services
{
    public class EventReprocessorService<TEvent> : IEventReprocessorService<TEvent> where TEvent : class, IDomainEvent
    {
        private readonly ILogger<EventReprocessorService<TEvent>> _logger;
        private readonly Publisher<TEvent> _publisher;
        private readonly IMongoCollection<DeadLetterEventDocument> _deadLetterStore;

        public EventReprocessorService(IMongoDatabase mongoDatabase, IPublisherFactory publisherFactory, ILogger<EventReprocessorService<TEvent>> logger)
        {
            _logger = logger;
            _publisher = publisherFactory.CreatePublisher<TEvent>();
            _deadLetterStore = mongoDatabase.GetCollection<DeadLetterEventDocument>("deadLetterEvents");
        }

        public async Task ReprocessAllAsync()
        {
            var documents = await _deadLetterStore.Find(evt => evt.EventType == typeof(TEvent).FullName).ToListAsync();

            foreach (DeadLetterEventDocument doc in documents)
            {
                var eventType = Type.GetType(doc.EventType);
                if (eventType is null)
                {
                    _logger.LogWarning("Unknown event type: {Type}", doc.EventType.ToString());
                    continue;
                }

                if (eventType != null && typeof(IDomainEvent).IsAssignableFrom(eventType))
                {
                    var @event = (TEvent)BsonSerializer.Deserialize(doc.EventData, eventType);
                    if (@event is null) continue;

                    await _publisher.PublishAsync(@event);
                    await _deadLetterStore.DeleteOneAsync(d => d.Id == doc.Id);

                    _logger.LogInformation("Reprocessed {EventType}", doc.EventType);
                }
            }
        }
    }
}