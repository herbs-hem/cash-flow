using CashFlow.Api.Domain.Enums;

namespace PocCQRS.Domain.Entities;

public sealed record Transaction(
    Guid BankAccountId, decimal Amount, OperationType OperationType,
    string Description, DateTime Date, Guid TransactionId = default!);