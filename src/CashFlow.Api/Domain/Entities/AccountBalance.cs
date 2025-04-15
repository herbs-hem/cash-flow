namespace CashFlow.Api.Domain.Entities;

public sealed record AccountBalance(
    Guid CompanyAccountId, decimal InitialBalance, decimal FinalBalance, DateTime Date);