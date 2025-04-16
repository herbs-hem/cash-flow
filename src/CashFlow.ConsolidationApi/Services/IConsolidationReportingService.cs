using CashFlow.Application.Dtos;
using CashFlow.Application.Queries;
using CashFlow.ConsolidationApi.Models.Requests;
using CashFlow.Domain.Abstractions;

namespace CashFlow.ConsolidationApi.Services;

public interface IConsolidationReportingService
{
    public Task<HttpResult<List<ConsolidateDetailsDto>>> GetConsolidatedReport(ReportDailyRequest request); 
}