namespace PocCQRS.Domain.Entities;

public sealed record AccountBalance(
    Guid BankAccountId, decimal InitialBalance, decimal FinalBalance, DateTime Date);