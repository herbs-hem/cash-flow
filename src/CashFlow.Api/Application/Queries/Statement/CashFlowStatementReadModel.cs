using CashFlow.Api.Domain.Enums;

namespace CashFlow.Api.Application.Queries.Statement;

public sealed record CashFlowStatementReadModel(Guid CashierId, IEnumerable<StatementDetailsReadModel> Statements, decimal CurrentBalance);

public sealed record StatementDetailsReadModel(string Description, decimal Value, OperationType OperationType, DateTime Date);