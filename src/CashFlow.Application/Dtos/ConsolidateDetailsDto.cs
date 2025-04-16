namespace CashFlow.Application.Dtos;

public sealed record ConsolidateDetailsDto(
    Guid CompanyAccountId,
    DateTime Date,
    string Description,
    Microsoft.OpenApi.Models.OperationType OperationType,
    decimal Amount,
    decimal BalanceStartDay,
    decimal Balance);