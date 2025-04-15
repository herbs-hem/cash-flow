using MediatR;
using CashFlow.Api.Domain.Abstractions.Generic;
using CashFlow.Api.Domain.Enums;

namespace CashFlow.Api.Application.Queries.Statement;

public sealed record CashFlowStatementByDaysQuery(Guid CompanyAccountId, int Days) : IRequest<Result<CashFlowStatementReadModel>>
{
    public OperationType OperationType = OperationType.All;
}