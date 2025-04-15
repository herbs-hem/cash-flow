using MediatR;
using CashFlow.Api.Domain.Abstractions.Generic;
using CashFlow.Api.Domain.Constants;
using CashFlow.Api.Domain.Enums;

namespace CashFlow.Api.Application.Queries.Statement;

public sealed record CashFlowStatementAllQuery(Guid CompanyAccountId) : IRequest<Result<CashFlowStatementReadModel>>
{
    public int Days => StatementRulesConstant.MinimumDaysLimit;
    public OperationType OperationType = OperationType.All;
}