using MediatR;
using CashFlow.Api.Domain.Abstractions.Generic;

namespace CashFlow.Api.Application.Queries.Balance;

public sealed record CashFlowBalanceByCompanyAccountIdQuery(Guid CompanyAccountId) : IRequest<Result<CashFlowBalanceReadModel>>;