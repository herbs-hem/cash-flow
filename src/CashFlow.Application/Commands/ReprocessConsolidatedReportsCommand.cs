using MediatR;

namespace CashFlow.Application.Commands;

public record ReprocessConsolidatedReportsCommand(
    Guid CompanyAccountId,
    DateTime TransactionDate) : IRequest;