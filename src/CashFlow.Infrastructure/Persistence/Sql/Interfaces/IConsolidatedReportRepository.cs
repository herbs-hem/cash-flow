using CashFlow.Domain.Entities;

namespace CashFlow.Infrastructure.Persistence.Sql.Interfaces;

public interface IConsolidatedReportRepository
{
    Task<IList<ConsolidateDetails>> GetConsolidatedDetailsAsync(
        Guid companyAccountId, 
        DateTime initialDate, 
        DateTime endDate);
    
    Task<ConsolidateDetails?> GetLastConsolidatedDetailAsync(Guid companyAccountId);
}