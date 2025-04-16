namespace CashFlow.Domain.Entities;

public enum OperationType
{
	Credit = 1, 
	Debit = 2
}
public sealed record Transaction(Guid CompanyAccountId, decimal Amount,
OperationType OperationType, string Description, DateTime Date, Guid Id = default!);
