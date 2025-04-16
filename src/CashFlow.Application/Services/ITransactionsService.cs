namespace CashFlow.Application.Services
{
    public interface ITransactionsService
    {
        Task<Guid> InFlowAsync(Guid companyAccountId, decimal amount, string description, DateTime date, decimal balanceStartDay, decimal balanceEndDay);

        Task<Guid> OutFlowAsync(Guid companyAccountId, decimal amount, string description, DateTime date, decimal balanceStartDay, decimal balanceEndDay);
        
    }
}