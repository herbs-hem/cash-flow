using CashFlow.Application.Dtos;

namespace CashFlow.Application.Services;

public interface IReportingService
{
    Task<List<ConsolidateDetailsDto>> GetConsolidatedReportsAsync(Guid companyAccountId, DateTime startDate, DateTime endDate);
    Task ReprocessConsolidatedReportAsync(Guid companyAccountId, DateTime date);
}