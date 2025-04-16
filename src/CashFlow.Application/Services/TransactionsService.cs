using Microsoft.Extensions.Logging;
using CashFlow.Application.Dtos;
using CashFlow.Domain.Entities;
using CashFlow.Infrastructure.Persistence.Sql.Interfaces;
using OperationType = CashFlow.Application.Dtos.OperationType;

namespace CashFlow.Application.Services
{
    public class TransactionsService : ITransactionsService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountBalanceRepository _accountBalanceRepository;
        private readonly ILogger<TransactionsService> _logger;

        public TransactionsService(
            ITransactionRepository transactionRepository, IAccountBalanceRepository accountBalanceRepository,
            ILogger<TransactionsService> logger)
        {
            _transactionRepository = transactionRepository;
            _accountBalanceRepository = accountBalanceRepository;
            _logger = logger;
        }

        public async Task<Guid> InFlowAsync(
             Guid companyAccountId, decimal amount, string description, DateTime date, decimal balanceStartDay, decimal balanceEndDay)
        {
            var accountBalance = new AccountBalance(companyAccountId, balanceStartDay, balanceEndDay, date);

            var id = await _accountBalanceRepository.UpsertAsync(accountBalance);

            var transaction = new Transaction(companyAccountId, amount, CashFlow.Domain.Entities.OperationType.Credit, description, date);

            var transactionId = await _transactionRepository.InsertAsync(transaction);

            return transactionId;
        }

        public async Task<Guid> OutFlowAsync(
            Guid companyAccountId, decimal amount, string description, DateTime date, decimal balanceStartDay, decimal balanceEndDay)
        {
            var accountBalance = new AccountBalance(companyAccountId, balanceStartDay, balanceEndDay, date);

            var id = await _accountBalanceRepository.UpsertAsync(accountBalance);

            var transaction = new Transaction(companyAccountId, amount, CashFlow.Domain.Entities.OperationType.Debit, description, date);

            var transactionId = await _transactionRepository.InsertAsync(transaction);

            return transactionId;
        }
    }
}