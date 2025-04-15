using CashFlow.Api.Domain.Abstractions;

namespace CashFlow.Api.Application.Queries.Statement;

public static class CashFlowStatementErrors
{
    public static readonly Error RequiredRequest = new("CashFlowStatement.RequiredRequest", "Request required and it should be filled!");

    public static Error WithInvalidCompanyAccountId(Guid cashierId) => new("CashFlowStatement.WithInvalidCompanyAccountId", $"CompanyAccountId {cashierId} is not valid!");

    public static Error CompanyAccountIdNotFound(Guid cachierId) => new("CashFlowStatement.NotFoundByCompanyAccountId", $"CompanyAccountId {cachierId} not found!");

    public static Error FailedValidationRules(string description) => new("CashFlowStatement.FailedValidationRules", description);
}