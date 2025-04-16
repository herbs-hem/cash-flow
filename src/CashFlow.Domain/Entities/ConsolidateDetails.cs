namespace CashFlow.Domain.Entities;

public sealed record ConsolidateDetails(
    Guid CompanyAccountId,
    DateTime Date,
    string Description,
    Microsoft.OpenApi.Models.OperationType OperationType,
    decimal Amount,
    decimal BalanceStartDay,
    decimal Balance);