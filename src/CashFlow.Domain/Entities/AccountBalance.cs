namespace CashFlow.Domain.Entities;

public sealed record AccountBalance(
    Guid CompanyAccountId, decimal InitialBalance, decimal FinalBalance, DateTime Date, Guid Id = default!);
