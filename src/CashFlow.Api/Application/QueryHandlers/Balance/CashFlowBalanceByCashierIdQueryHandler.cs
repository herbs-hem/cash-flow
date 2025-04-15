using MediatR;
using CashFlow.Api.Application.Queries.Balance;
using CashFlow.Api.Application.Validators;
using CashFlow.Api.Domain.Abstractions;
using CashFlow.Api.Domain.Abstractions.Extensions;
using CashFlow.Api.Domain.Abstractions.Generic;
using CashFlow.Api.Domain.Services;
using CashFlow.Api.Infrastructure.Cache.Interfaces;

namespace CashFlow.Api.Application.QueryHandlers.Balance;

public class CashFlowBalanceByCashierIdQueryHandler :
    IRequestHandler<CashFlowBalanceByCashierIdQuery, Result<CashFlowBalanceReadModel>>,
    IQueryHandler<CashFlowBalanceByCashierIdQuery, Result>
{
    private readonly ICashFlowTransactionService _service;
    private readonly ICacheClient _cacheClient;
    private readonly ILogger<CashFlowBalanceByCashierIdQueryHandler> _logger;

    public CashFlowBalanceByCashierIdQueryHandler(
        ICashFlowTransactionService service, ICacheClient cacheClient,
        ILogger<CashFlowBalanceByCashierIdQueryHandler> logger)
    {
        _service = service;
        _cacheClient = cacheClient;
        _logger = logger;
    }

    public async Task<Result<CashFlowBalanceReadModel>> Handle(CashFlowBalanceByCashierIdQuery request, CancellationToken cancellationToken)
    {
        var result = await ValidateAsync(request);

        if (result.IsFailure)
            return result.ToResult<CashFlowBalanceReadModel>();

        var balanceCachekey = $"BalanceQuery_{request.CashierId}_{DateTime.UtcNow.Date:yyyyMMdd}";

        _cacheClient.TryGetValue(balanceCachekey, out CashFlowBalanceReadModel balance);

        balance ??= await _service.GetBalanceAsync(request.CashierId);

        if (balance != null)
            await _cacheClient.SetAsync(balanceCachekey, balance);

        return Result.Success(balance ?? new CashFlowBalanceReadModel(request.CashierId, 0));
    }

    public async Task<Result> ValidateAsync(CashFlowBalanceByCashierIdQuery request)
    {
        if (request == null)
            return CashFlowBalanceErrors.RequiredRequest;

        if (request.CashierId == Guid.Empty)
            return CashFlowBalanceErrors.WithInvalidCashierId(request.CashierId);

        var validationResults
            = await new AutoValidator()
                .Build<CashFlowBalanceByCashierIdQuery>()
                .With(ctx => ctx.CashierId, async value => await _service.BankAccountExistsAsync(value), $"{request.CashierId} not found!")
                .ValidateAsync(request);

        if (!validationResults.IsValid)
        {
            string errorMessage = string.Join(Environment.NewLine, validationResults.Errors.Select(e => $"{e.Key}: {e.Value}"));

            return CashFlowBalanceErrors.FailedValidationRules(errorMessage);
        }

        return Result.Success();
    }
}