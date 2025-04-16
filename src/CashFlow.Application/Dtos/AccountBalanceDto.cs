namespace CashFlow.Application.Dtos;

public sealed record AccountBalanceDto(
    Guid CompanyAccountId, decimal InitialBalance, decimal FinalBalance, DateTime Date);
