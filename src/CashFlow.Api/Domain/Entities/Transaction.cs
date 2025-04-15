using CashFlow.Api.Domain.Enums;

namespace CashFlow.Api.Domain.Entities;

public sealed record Transaction(
    Guid CompanyAccountId, decimal Amount, OperationType OperationType,
    string Description, DateTime Date, Guid TransactionId = default!);