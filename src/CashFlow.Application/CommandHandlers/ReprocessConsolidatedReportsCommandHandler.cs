using CashFlow.Application.Commands;
using CashFlow.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CashFlow.Application.CommandHandlers;

public class ReprocessConsolidatedReportsCommandHandler : IRequestHandler<ReprocessConsolidatedReportsCommand>
{
    private readonly IReportingService _reportingService;
    private readonly ILogger<ReprocessConsolidatedReportsCommandHandler> _logger;

    public ReprocessConsolidatedReportsCommandHandler(
        IReportingService reportingService,
        ILogger<ReprocessConsolidatedReportsCommandHandler> logger)
    {
        _reportingService = reportingService;
        _logger = logger;
    }

    public async Task Handle(ReprocessConsolidatedReportsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _reportingService.ReprocessConsolidatedReportAsync(
                request.CompanyAccountId,
                request.TransactionDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to reprocess consolidated reports for company {CompanyAccountId} and date {Date}", 
                request.CompanyAccountId, 
                request.TransactionDate.ToString("yyyy-MM-dd"));
            throw;
        }
    }
}