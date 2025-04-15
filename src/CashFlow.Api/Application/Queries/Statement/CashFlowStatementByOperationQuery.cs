using MediatR;
using CashFlow.Api.Domain.Abstractions.Generic;
using CashFlow.Api.Domain.Constants;
using CashFlow.Api.Domain.Enums;

namespace CashFlow.Api.Application.Queries.Statement;

public sealed record CashFlowStatementByOperationQuery(Guid CompanyAccountId, OperationType OperationType) : IRequest<Result<CashFlowStatementReadModel>>
{
    public int Days => StatementRulesConstant.MinimumDaysLimit;
}