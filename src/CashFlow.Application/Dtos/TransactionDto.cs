namespace CashFlow.Application.Dtos;

public enum OperationType
{
	Credit = 1, 
	Debit = 2
}
public sealed record TransactionDto(Guid CompanyAccountId, decimal Amount,
OperationType OperationType, string Description, DateTime Date, Guid Id = default!);
