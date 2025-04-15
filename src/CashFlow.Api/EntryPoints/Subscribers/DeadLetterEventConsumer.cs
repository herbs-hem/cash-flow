using CashFlow.Api.Domain.Events;
using CashFlow.Api.Infrastructure.Persistence.NoSql.Models;
using MassTransit;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;

namespace CashFlow.Api.EntryPoint.Consumer;

public class DeadLetterEventConsumer<T> : IConsumer<T> where T : class, IDomainEvent
{
    private readonly ILogger<DeadLetterEventConsumer<T>> _logger;
    private readonly IMongoCollection<DeadLetterEventDocument> _deadLetterEventStore;

    public DeadLetterEventConsumer(ILogger<DeadLetterEventConsumer<T>> logger, IMongoDatabase mongoDatabase)
    {
        _logger = logger;
        _deadLetterEventStore = mongoDatabase.GetCollection<DeadLetterEventDocument>("deadLetterEvents");
    }

    public async Task Consume(ConsumeContext<T> context)
    {
        _logger.LogWarning("DLQ Received: {EventType} - Message: {Message}",
            typeof(T).Name, JsonSerializer.Serialize(context.Message));

        var document = new DeadLetterEventDocument
        {
            Id = Guid.NewGuid(),
            EventType = typeof(T).FullName!,
            CreatedAt = DateTime.UtcNow,
            EventData = context.Message.ToBsonDocument(typeof(T))
        };

        await _deadLetterEventStore.InsertOneAsync(document);
    }
}