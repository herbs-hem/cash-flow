using CashFlow.Api.Application.Queries.Statement;
using CashFlow.Api.Application.Validators;
using CashFlow.Api.Domain.Abstractions;
using CashFlow.Api.Domain.Abstractions.Extensions;
using CashFlow.Api.Domain.Abstractions.Generic;
using CashFlow.Api.Domain.Constants;
using CashFlow.Api.Domain.Enums;
using CashFlow.Api.Domain.Services;
using CashFlow.Api.Infrastructure.Cache.Interfaces;
using MediatR;
using Microsoft.OpenApi.Extensions;
using Newtonsoft.Json;

namespace CashFlow.Api.Application.QueryHandlers.Statement;

public class CashFlowStatementByDaysAndOperationQueryHandler :
    IRequestHandler<CashFlowStatementByDaysAndOperationQuery, Result<CashFlowStatementReadModel>>,
    IQueryHandler<CashFlowStatementByDaysAndOperationQuery, Result>
{
    private readonly ICashFlowTransactionService _service;
    private readonly ICacheClient _cacheClient;
    private readonly ILogger<CashFlowStatementByDaysAndOperationQueryHandler> _logger;

    public CashFlowStatementByDaysAndOperationQueryHandler(
        ICashFlowTransactionService service, ICacheClient cacheClient,
        ILogger<CashFlowStatementByDaysAndOperationQueryHandler> logger)
    {
        _service = service;
        _cacheClient = cacheClient;
        _logger = logger;
    }

    public async Task<Result<CashFlowStatementReadModel>> Handle(CashFlowStatementByDaysAndOperationQuery request, CancellationToken cancellationToken)
    {
        var result = await ValidateAsync(request);

        if (result.IsFailure)
            return result.ToResult<CashFlowStatementReadModel>();

        var statementCacheKey = $"{request.Days}_{request.OperationType.GetDisplayName()}StatementQuery_{request.CompanyAccountId}_{DateTime.UtcNow.Date:yyyyMMdd}";

        _cacheClient.TryGetValue(statementCacheKey, out CashFlowStatementReadModel statement);
        _logger.LogDebug("Try get statement on cache! Statement: {Statement}", JsonConvert.SerializeObject(statement));

        statement ??= await _service.GetStatementAsync(request.CompanyAccountId, request.Days, request.OperationType);

        if (statement != null)
        {
            await _cacheClient.SetAsync(statementCacheKey, statement);
            _logger.LogDebug("Set statement into cache! CacheKey: {CacheKey} - Statement: {Statement}", statementCacheKey, JsonConvert.SerializeObject(statement));
        }

        return Result.Success(statement ?? new CashFlowStatementReadModel(request.CompanyAccountId, Statements: [], 0));
    }

    public async Task<Result> ValidateAsync(CashFlowStatementByDaysAndOperationQuery request)
    {
        if (request == null)
            return CashFlowStatementErrors.RequiredRequest;

        if (request.CompanyAccountId == Guid.Empty)
            return CashFlowStatementErrors.WithInvalidCompanyAccountId(request.CompanyAccountId);

        var validationResults
            = await new AutoValidator()
                .Build<CashFlowStatementByDaysAndOperationQuery>()
                .With(ctx => ctx.CompanyAccountId, async value => await _service.BankAccountExistsAsync(value), $"{request.CompanyAccountId} not found! ")
                .With(ctx => ctx.Days, value => value >= StatementRulesConstant.MinimumDaysLimit && value <= StatementRulesConstant.MaximumDaysLimit, $"Days outside the allowed range ({StatementRulesConstant.MinimumDaysLimit} to {StatementRulesConstant.MaximumDaysLimit})")
                .With(ctx => ctx.OperationType, value => OperationType.All != value, errorMessage: "Query only allows Inflow or Outflow transactions!")
                .ValidateAsync(request);

        if (!validationResults.IsValid)
        {
            string errorMessage = string.Join(Environment.NewLine, validationResults.Errors.Select(e => $"{e.Key}: {e.Value}"));

            return CashFlowStatementErrors.FailedValidationRules(errorMessage);
        }

        return Result.Success();
    }
}