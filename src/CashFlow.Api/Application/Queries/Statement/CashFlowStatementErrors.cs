using CashFlow.Api.Domain.Abstractions;

namespace CashFlow.Api.Application.Queries.Statement;

public static class CashFlowStatementErrors
{
    public static readonly Error RequiredRequest = new("CashFlowStatement.RequiredRequest", "Request required and it should be filled!");

    public static Error WithInvalidCashierId(Guid cashierId) => new("CashFlowStatement.WithInvalidCashierId", $"CashierId {cashierId} is not valid!");

    public static Error CashierIdNotFound(Guid cachierId) => new("CashFlowStatement.NotFoundByCashierId", $"CashierId {cachierId} not found!");

    public static Error FailedValidationRules(string description) => new("CashFlowStatement.FailedValidationRules", description);
}