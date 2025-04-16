using AutoMapper;
using CashFlow.Application.Dtos;
using CashFlow.Application.Queries;
using CashFlow.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using CashFlow.Domain.Abstractions;
using CashFlow.Infrastructure.Persistence.Cache.Interfaces;
using CashFlow.Infrastructure.Persistence.Sql.Interfaces;
using CashFlow.Infrastructure.Persistence.Sql.Repository;
using Google.Protobuf.WellKnownTypes;

namespace CashFlow.Application.QueryHadlers;

public class GetConsolidatedDataQueryHandler : IRequestHandler<GetConsolidatedDataQuery, List<ConsolidateDetailsDto>>
{
    private readonly IReportingService _reportingService;
    private static readonly TimeSpan @TimeSpan = TimeSpan.FromHours(1);
    // private readonly IConsolidatedReportRepository _reportRepository;
    // private readonly IAsyncCacheClient _cacheClient;
    private readonly ILogger<GetConsolidatedDataQueryHandler> _logger;
    // private readonly IMapper _mapper;

    public GetConsolidatedDataQueryHandler
    (
        IReportingService reportingService,
        // IConsolidatedReportRepository reportRepository,
        // IAsyncCacheClient cacheClient,
        // IMapper mapper,
        ILogger<GetConsolidatedDataQueryHandler> logger)
    {
        _reportingService = reportingService;
        // _reportRepository = reportRepository;
        // _cacheClient = cacheClient;
        // _mapper = mapper;
        _logger = logger;
    }

    public Task<List<ConsolidateDetailsDto>> Handle(GetConsolidatedDataQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return _reportingService.GetConsolidatedReportsAsync(request.CompanyAccountId, request.InitialDate,
                request.EndDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }
}