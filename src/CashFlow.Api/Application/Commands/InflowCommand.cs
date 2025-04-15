using MediatR;

namespace CashFlow.Api.Application.Commands;

public sealed record InflowCommand(decimal Amount, Guid BankAccountId = default!, string Description = default!) : IRequest<Guid>;
