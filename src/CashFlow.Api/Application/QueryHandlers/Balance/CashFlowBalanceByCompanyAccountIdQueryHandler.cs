using CashFlow.Api.Application.Queries.Balance;
using CashFlow.Api.Application.Validators;
using CashFlow.Api.Domain.Abstractions;
using CashFlow.Api.Domain.Abstractions.Extensions;
using CashFlow.Api.Domain.Abstractions.Generic;
using CashFlow.Api.Domain.Services;
using CashFlow.Api.Infrastructure.Cache.Interfaces;
using MediatR;
using Newtonsoft.Json;

namespace CashFlow.Api.Application.QueryHandlers.Balance;

public class CashFlowBalanceByCompanyAccountIdQueryHandler :
    IRequestHandler<CashFlowBalanceByCompanyAccountIdQuery, Result<CashFlowBalanceReadModel>>,
    IQueryHandler<CashFlowBalanceByCompanyAccountIdQuery, Result>
{
    private readonly ICashFlowTransactionService _service;
    private readonly ICacheClient _cacheClient;
    private readonly ILogger<CashFlowBalanceByCompanyAccountIdQueryHandler> _logger;

    public CashFlowBalanceByCompanyAccountIdQueryHandler(
        ICashFlowTransactionService service, ICacheClient cacheClient,
        ILogger<CashFlowBalanceByCompanyAccountIdQueryHandler> logger)
    {
        _service = service;
        _cacheClient = cacheClient;
        _logger = logger;
    }

    public async Task<Result<CashFlowBalanceReadModel>> Handle(CashFlowBalanceByCompanyAccountIdQuery request, CancellationToken cancellationToken)
    {
        var result = await ValidateAsync(request);

        if (result.IsFailure)
            return result.ToResult<CashFlowBalanceReadModel>();

        var balanceCachekey = $"BalanceQuery_{request.CompanyAccountId}_{DateTime.UtcNow.Date:yyyyMMdd}";

        _cacheClient.TryGetValue(balanceCachekey, out CashFlowBalanceReadModel balance);
        _logger.LogDebug("Try get balance on cache! Balance: {Balance}", JsonConvert.SerializeObject(balance));

        balance ??= await _service.GetBalanceAsync(request.CompanyAccountId);

        if (balance != null)
        {
            await _cacheClient.SetAsync(balanceCachekey, balance);
            _logger.LogDebug("Set balance into cache! CacheKey: {CacheKey} - Balance: {Balance}", balanceCachekey, JsonConvert.SerializeObject(balance));
        }

        return Result.Success(balance ?? new CashFlowBalanceReadModel(request.CompanyAccountId, 0));
    }

    public async Task<Result> ValidateAsync(CashFlowBalanceByCompanyAccountIdQuery request)
    {
        if (request == null)
            return CashFlowBalanceErrors.RequiredRequest;

        if (request.CompanyAccountId == Guid.Empty)
            return CashFlowBalanceErrors.WithInvalidCompanyAccountId(request.CompanyAccountId);

        var validationResults
            = await new AutoValidator()
                .Build<CashFlowBalanceByCompanyAccountIdQuery>()
                .With(ctx => ctx.CompanyAccountId, async value => await _service.CompanyAccountExistsAsync(value), $"{request.CompanyAccountId} not found!")
                .ValidateAsync(request);

        if (!validationResults.IsValid)
        {
            string errorMessage = string.Join(Environment.NewLine, validationResults.Errors.Select(e => $"{e.Key}: {e.Value}"));

            return CashFlowBalanceErrors.FailedValidationRules(errorMessage);
        }

        return Result.Success();
    }
}