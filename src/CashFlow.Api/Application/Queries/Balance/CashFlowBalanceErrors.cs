using CashFlow.Api.Domain.Abstractions;

namespace CashFlow.Api.Application.Queries.Balance;

public static class CashFlowBalanceErrors
{
    public static readonly Error RequiredRequest = new("CashFlowBalance.RequiredRequest", "Request required and it should be filled!");

    public static Error WithInvalidCompanyAccountId(Guid companyAccountId) => new("CashFlowBalance.WithInvalidCompanyAccountId", $"CompanyAccountId {companyAccountId} is not valid!");

    public static Error CompanyAccountIdNotFound(Guid companyAccountId) => new("CashFlowBalance.NotFoundByCompanyAccountId", $"CompanyAccountId {companyAccountId} not found!");

    public static Error FailedValidationRules(string description) => new("CashFlowBalance.FailedValidationRules", description);
}