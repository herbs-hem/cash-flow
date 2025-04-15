using CashFlow.Api.Domain.Abstractions;

namespace CashFlow.Api.Application.Queries.Balance;

public static class CashFlowBalanceErrors
{
    public static readonly Error RequiredRequest = new("CashFlowBalance.RequiredRequest", "Request required and it should be filled!");

    public static Error WithInvalidCashierId(Guid bankAccountId) => new("CashFlowBalance.WithInvalidCashierId", $"CashierId {bankAccountId} is not valid!");

    public static Error CashierIdNotFound(Guid bankAccountId) => new("CashFlowBalance.NotFoundByCashierId", $"CashierId {bankAccountId} not found!");

    public static Error FailedValidationRules(string description) => new("CashFlowBalance.FailedValidationRules", description);
}