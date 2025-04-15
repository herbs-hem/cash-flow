namespace CashFlow.Api.Application.Queries.Balance;

public sealed record CashFlowBalanceReadModel(Guid BankAccountId, decimal Balance);