using MediatR;

namespace CashFlow.Api.Application.Commands;

public sealed record OutflowCommand(decimal Amount, Guid CompanyAccountId = default!, string Description = default!) : IRequest<Guid>;