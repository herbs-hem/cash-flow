using MediatR;
using CashFlow.Api.Domain.Abstractions.Generic;
using CashFlow.Api.Domain.Enums;

namespace CashFlow.Api.Application.Queries.Statement;

public sealed record CashFlowStatementByDaysAndOperationQuery(Guid CompanyAccountId, int Days, OperationType OperationType) : IRequest<Result<CashFlowStatementReadModel>>;