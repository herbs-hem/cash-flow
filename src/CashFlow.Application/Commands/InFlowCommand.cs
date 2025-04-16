using MediatR;
using CashFlow.Domain.Abstractions;

namespace CashFlow.Application.Commands
{
    public sealed record InFlowCommand(decimal Amount, Guid CompanyAccountId = default!, string Description = default!) : IRequest<Guid>;
}
