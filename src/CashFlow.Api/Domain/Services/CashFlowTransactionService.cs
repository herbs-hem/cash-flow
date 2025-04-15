using CashFlow.Api.Application.Queries.Balance;
using CashFlow.Api.Application.Queries.Statement;
using CashFlow.Api.Domain.Entities;
using CashFlow.Api.Domain.Enums;
using CashFlow.Api.Domain.Exceptions;
using CashFlow.Api.Infrastructure.Persistence.Sql.Interfaces;

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
             Guid bankAccountId, decimal amount, string description, DateTime date, decimal balanceStartDay, decimal balanceEndDay)
        {
            var accountBalance = new AccountBalance(bankAccountId, balanceStartDay, balanceEndDay, date);

            var id = await _accountBalanceRepository.UpsertAsync(accountBalance);

            var transaction = new Transaction(bankAccountId, amount, OperationType.Inflow, description, date);

            var transactionId = await _transactionRepository.InsertAsync(transaction);

            return transactionId;
        }

        public async Task<Guid> DebitAsync(
            Guid bankAccountId, decimal amount, string description, DateTime date, decimal balanceStartDay, decimal balanceEndDay)
        {
            var accountBalance = new AccountBalance(bankAccountId, balanceStartDay, balanceEndDay, date);

            var id = await _accountBalanceRepository.UpsertAsync(accountBalance);

            var transaction = new Transaction(bankAccountId, amount, OperationType.Outflow, description, date);

            var transactionId = await _transactionRepository.InsertAsync(transaction);

            return transactionId;
        }

        public async Task<CashFlowBalanceReadModel> GetBalanceAsync(Guid bankAccountId)
        {
            var accountBalance = await _accountBalanceRepository.GetByAsync(bankAccountId);

            return await Task.FromResult(new CashFlowBalanceReadModel(accountBalance.BankAccountId, accountBalance.FinalBalance));
        }

        public async Task<CashFlowStatementReadModel> GetStatementAsync(Guid bankAccountId, int days, OperationType operationType)
        {
            // BR: minimum days limit to get a bank statement
            days = days <= 0 ? 15 : days;

            // BR: maximum days limit to get a bank statement
            if (days > 90)
                throw new DomainException(
                    $"{days} days exceed days limit to get the bank statement. Consider as valid amount of days: 15, 30, 60 and 90 days");

            var accountBalance = await _accountBalanceRepository.GetByAsync(bankAccountId);

            var previousDateToSearch = DateTime.UtcNow.AddDays(-days);

            IEnumerable<Transaction> transactions
                = await _transactionRepository.GetByAsync(bankAccountId, previousDateToSearch, DateTime.UtcNow, operationType);

            IList<StatementDetailsReadModel> statemets = [];
            foreach (Transaction transaction in transactions)
                statemets.Add(new StatementDetailsReadModel(transaction.Description, transaction.Amount, transaction.OperationType, transaction.Date));

            return await Task.FromResult(new CashFlowStatementReadModel(bankAccountId, statemets, accountBalance?.FinalBalance ?? 0));
        }
    }
}