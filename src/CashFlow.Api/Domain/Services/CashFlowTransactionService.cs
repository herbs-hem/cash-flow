using CashFlow.Api.Application.Queries.Balance;
using CashFlow.Api.Application.Queries.Statement;
using CashFlow.Api.Domain.Entities;
using CashFlow.Api.Domain.Enums;
using CashFlow.Api.Domain.Exceptions;
using CashFlow.Api.Infrastructure.Persistence.Sql.Interfaces;
using Microsoft.OpenApi.Extensions;

namespace CashFlow.Api.Domain.Services
{
    public class CashFlowTransactionService : ICashFlowTransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountBalanceRepository _accountBalanceRepository;
        private readonly ILogger<CashFlowTransactionService> _logger;

        public CashFlowTransactionService(
            ITransactionRepository transactionRepository, IAccountBalanceRepository accountBalanceRepository,
            ILogger<CashFlowTransactionService> logger)
        {
            _transactionRepository = transactionRepository;
            _accountBalanceRepository = accountBalanceRepository;
            _logger = logger;
        }

        public async Task<bool> BankAccountExistsAsync(Guid value)
        {
            return await _accountBalanceRepository.ExistsByAsync(value);
        }

        public async Task<Guid> CreditAsync(
             Guid companyAccountId, decimal amount, string description, DateTime date, decimal balanceStartDay, decimal balanceEndDay)
        {
            var accountBalance = new AccountBalance(companyAccountId, balanceStartDay, balanceEndDay, date);

            var id = await _accountBalanceRepository.UpsertAsync(accountBalance);
            _logger.LogDebug("Cash Flow balance has been upserted. UpdatedBalance: {UpdatedBalance} - OperationType: {OperationType}", id, OperationType.Inflow.GetDisplayName());

            var transaction = new Transaction(companyAccountId, amount, OperationType.Inflow, description, date);

            var transactionId = await _transactionRepository.InsertAsync(transaction);
            _logger.LogDebug("New cash flow transaction has been inserted. TransactionId: {TransactionId} - OperationType: {OperationType}", transactionId, OperationType.Inflow.GetDisplayName());

            return transactionId;
        }

        public async Task<Guid> DebitAsync(
            Guid companyAccountId, decimal amount, string description, DateTime date, decimal balanceStartDay, decimal balanceEndDay)
        {
            var accountBalance = new AccountBalance(companyAccountId, balanceStartDay, balanceEndDay, date);

            var id = await _accountBalanceRepository.UpsertAsync(accountBalance);
            _logger.LogDebug("Cash Flow balance has been upserted. UpdatedBalance: {UpdatedBalance} - OperationType: {OperationType}", id, OperationType.Outflow.GetDisplayName());

            var transaction = new Transaction(companyAccountId, amount, OperationType.Outflow, description, date);

            var transactionId = await _transactionRepository.InsertAsync(transaction);
            _logger.LogDebug("New cash flow transaction has been inserted. TransactionId: {TransactionId} - OperationType: {OperationType}", transactionId, OperationType.Outflow.GetDisplayName());

            return transactionId;
        }

        public async Task<CashFlowBalanceReadModel> GetBalanceAsync(Guid companyAccountId)
        {
            var accountBalance = await _accountBalanceRepository.GetByAsync(companyAccountId);

            return await Task.FromResult(new CashFlowBalanceReadModel(accountBalance.CompanyAccountId, accountBalance.FinalBalance));
        }

        public async Task<CashFlowStatementReadModel> GetStatementAsync(Guid companyAccountId, int days, OperationType operationType)
        {
            // BR: minimum days limit to get a bank statement
            days = days <= 0 ? 15 : days;

            // BR: maximum days limit to get a bank statement
            if (days >= 90)
                throw new DomainException(
                    $"{days} days exceed days limit to get the bank statement. Consider as valid amount of days: 15, 30, 60 and 90 days");

            var accountBalance = await _accountBalanceRepository.GetByAsync(companyAccountId);

            var previousDateToSearch = DateTime.UtcNow.AddDays(-days);

            IEnumerable<Transaction> transactions
                = await _transactionRepository.GetByAsync(companyAccountId, previousDateToSearch, DateTime.UtcNow, operationType);

            IList<StatementDetailsReadModel> statemets = [];
            foreach (Transaction transaction in transactions)
                statemets.Add(new StatementDetailsReadModel(transaction.Description, transaction.Amount, transaction.OperationType, transaction.Date));

            return await Task.FromResult(new CashFlowStatementReadModel(companyAccountId, statemets, accountBalance?.FinalBalance ?? 0));
        }
    }
}