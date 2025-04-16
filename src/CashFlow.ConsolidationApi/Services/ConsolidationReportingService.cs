using CashFlow.Application.Dtos;
using CashFlow.Application.Queries;
using CashFlow.ConsolidationApi.Models.Requests;
using CashFlow.Domain.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CashFlow.ConsolidationApi.Services;

public class ConsolidationReportingService : IConsolidationReportingService
{
    private readonly IMediator _mediator;
    private readonly ILogger<ConsolidationReportingService> _logger;

    public ConsolidationReportingService(
        IMediator mediator,
        ILogger<ConsolidationReportingService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<HttpResult<List<ConsolidateDetailsDto>>> GetConsolidatedReport(ReportDailyRequest request)
    {
        try
        {
            if (request == null)
            {
                return HttpResult<List<ConsolidateDetailsDto>>.BadRequest(
                    new Error("Invalid request You must specify a valid query"));
            }
    
            if (request.InitialDate > request.EndDate)
            {
                return HttpResult<List<ConsolidateDetailsDto>>.BadRequest(
                    new Error("Invalid date range Initial date cannot be greater than end date"));
            }
    
            var query = new GetConsolidatedDataQuery(
                request.CompanyAccountId, 
                request.InitialDate, 
                request.EndDate);
            
            var response = await _mediator.Send(query);
            
            return HttpResult<List<ConsolidateDetailsDto>>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing consolidated report");
            return HttpResult<List<ConsolidateDetailsDto>>.InternalServerError(
                new Error("ServerError An unexpected error occurred while processing your request"));
        }
    }
}