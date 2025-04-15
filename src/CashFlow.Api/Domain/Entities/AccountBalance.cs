namespace CashFlow.Api.Domain.Entities;

public sealed record AccountBalance(
    Guid BankAccountId, decimal InitialBalance, decimal FinalBalance, DateTime Date);