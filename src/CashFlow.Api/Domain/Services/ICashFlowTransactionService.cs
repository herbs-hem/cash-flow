using CashFlow.Api.Application.Queries.Balance;
using CashFlow.Api.Application.Queries.Statement;
using CashFlow.Api.Domain.Enums;

namespace CashFlow.Api.Domain.Services
{
    public interface ICashFlowTransactionService
    {
        Task<bool> BankAccountExistsAsync(Guid value);

        Task<Guid> CreditAsync(Guid bankAccountId, decimal amount, string description, DateTime date, decimal balanceStartDay, decimal balanceEndDay);

        Task<Guid> DebitAsync(Guid bankAccountId, decimal amount, string description, DateTime date, decimal balanceStartDay, decimal balanceEndDay);

        Task<CashFlowBalanceReadModel> GetBalanceAsync(Guid bankAccountId);

        Task<CashFlowStatementReadModel> GetStatementAsync(Guid bankAccountId, int days, OperationType operationType);
    }
}