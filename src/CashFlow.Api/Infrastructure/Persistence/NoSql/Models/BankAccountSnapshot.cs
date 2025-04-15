using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CashFlow.Api.Infrastructure.Persistence.NoSql.Models;

[BsonIgnoreExtraElements]
public class BankAccountSnapshot
{
    [BsonId]
    [BsonElement("AggregateId")]
    [BsonRepresentation(BsonType.String)]
    public string AggregateId { get; set; } = default!;

    [BsonRepresentation(BsonType.String)]
    public Guid CompanyAccountId { get; set; } = default!;

    [BsonElement("Date")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Date { get; set; } = default!;

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal BalanceStart { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal BalanceEnd { get; set; }

    [BsonElement("LastEventId")]
    [BsonRepresentation(BsonType.String)]
    public Guid LastEventId { get; set; } = default!;

    [BsonElement("AggregateData")]
    public BsonDocument AggregateData { get; set; } = default!;// JSON do aggregate
}