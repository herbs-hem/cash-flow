namespace CashFlow.Api.Application.Queries.Balance;

public sealed record CashFlowBalanceReadModel(Guid CompanyAccountId, decimal Balance);