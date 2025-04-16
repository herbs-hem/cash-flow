using System.Globalization;
using AutoMapper;
using CashFlow.Application.Dtos;
using CashFlow.Application.QueryHadlers;
using CashFlow.Infrastructure.Persistence.Cache.Interfaces;
using CashFlow.Infrastructure.Persistence.Sql.Interfaces;
using Microsoft.Extensions.Logging;

namespace CashFlow.Application.Services;

public class ReportingService : IReportingService
{
    private static readonly TimeSpan @TimeSpan = TimeSpan.FromHours(1);
    
    private readonly IConsolidatedReportRepository _reportRepository;
    private readonly ICacheClient _cacheClient;
    private readonly ILogger<GetConsolidatedDataQueryHandler> _logger;
    private readonly IMapper _mapper;

    public ReportingService(IConsolidatedReportRepository reportRepository, 
        ICacheClient cacheClient,
        IMapper mapper,
        ILogger<GetConsolidatedDataQueryHandler> logger)
    {
        _reportRepository = reportRepository;
        _cacheClient = cacheClient;
        _mapper = mapper;
        _logger = logger;
    }
    
    public Task<List<ConsolidateDetailsDto>> GetConsolidatedReportsAsync(Guid companyAccountId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var cacheKey = $"consolidated-report:{companyAccountId}:{startDate:yyyy-MM-dd}:{endDate:yyyy-MM-dd}";
            
            return _cacheClient.GetOrCreateAsync(cacheKey, () 
                    =>  Task.FromResult(_mapper.Map<List<ConsolidateDetailsDto>>( _reportRepository.GetConsolidatedDetailsAsync(companyAccountId, startDate, endDate))),
                @TimeSpan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    public async Task ReprocessConsolidatedReportAsync(Guid companyAccountId, DateTime date)
    {
        try
        {
            // Padrão para encontrar todas as chaves de relatório desta conta
            var pattern = $"consolidated-report:{companyAccountId}:*";
            var allReportKeys = await _cacheClient.FindKeysByPatternAsync(pattern);

            foreach (var key in allReportKeys)
            {
                var parts = key.Split(':');
                if (parts.Length == 4 && 
                    DateTime.TryParseExact(parts[2], "yyyy-MM-dd", null, DateTimeStyles.None, out var startDate) &&
                    DateTime.TryParseExact(parts[3], "yyyy-MM-dd", null, DateTimeStyles.None, out var endDate))
                {
                    // Verifica se a data da transação está dentro do intervalo do relatório
                    if (date >= startDate && date <= endDate)
                    {
                        // Recupera o TTL atual antes de atualizar
                        var ttl = _cacheClient.GetTimeToLive(key);
                    
                        // Busca os dados atualizados do repositório
                        var updatedData = _mapper.Map<List<ConsolidateDetailsDto>>(
                            await _reportRepository.GetConsolidatedDetailsAsync(companyAccountId, startDate, endDate));
                    
                        // Atualiza o cache mantendo o TTL original
                        await _cacheClient.UpsertAsync(key, updatedData, ttl);
                    
                        _logger.LogInformation("Updated consolidated report in cache for key {Key}", key);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reprocess consolidated reports for company {CompanyAccountId} and date {Date}", 
                companyAccountId, date.ToString("yyyy-MM-dd"));
            throw;
        }
    }
}