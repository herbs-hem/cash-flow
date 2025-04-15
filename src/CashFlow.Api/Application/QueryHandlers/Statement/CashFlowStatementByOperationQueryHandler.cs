using MediatR;
using Microsoft.OpenApi.Extensions;
using CashFlow.Api.Application.Queries.Statement;
using CashFlow.Api.Application.Validators;
using CashFlow.Api.Domain.Abstractions;
using CashFlow.Api.Domain.Abstractions.Extensions;
using CashFlow.Api.Domain.Abstractions.Generic;
using CashFlow.Api.Domain.Constants;
using CashFlow.Api.Domain.Entities;
using CashFlow.Api.Domain.Services;
using CashFlow.Api.Infrastructure.Cache.Interfaces;
using CashFlow.Api.Domain.Enums;
using Newtonsoft.Json;

namespace CashFlow.Api.Application.QueryHandlers.Statement;

public class CashFlowStatementByOperationQueryHandler :
    IRequestHandler<CashFlowStatementByOperationQuery, Result<CashFlowStatementReadModel>>,
    IQueryHandler<CashFlowStatementByOperationQuery, Result>
{
    private readonly ICashFlowTransactionService _service;
    private readonly ICacheClient _cacheClient;
    private readonly ILogger<CashFlowStatementByOperationQueryHandler> _logger;

    public CashFlowStatementByOperationQueryHandler(
        ICashFlowTransactionService service, ICacheClient cacheClient,
        ILogger<CashFlowStatementByOperationQueryHandler> logger)
    {
        _service = service;
        _cacheClient = cacheClient;
        _logger = logger;
    }

    public async Task<Result<CashFlowStatementReadModel>> Handle(CashFlowStatementByOperationQuery request, CancellationToken cancellationToken)
    {
        var result = await ValidateAsync(request);

        if (result.IsFailure)
            return result.ToResult<CashFlowStatementReadModel>();

        var statementCacheKey = $"{request.OperationType.GetDisplayName()}StatementQuery_{request.CompanyAccountId}_{DateTime.UtcNow.Date:yyyyMMdd}";

        _cacheClient.TryGetValue(statementCacheKey, out CashFlowStatementReadModel statement);
        _logger.LogDebug("Try get statement on cache! Statement: {Statement}", JsonConvert.SerializeObject(statement));

        statement ??= await _service.GetStatementAsync(request.CompanyAccountId, StatementRulesConstant.MinimumDaysLimit, request.OperationType);

        if (statement != null)
        {
            await _cacheClient.SetAsync(statementCacheKey, statement);
            _logger.LogDebug("Set statement into cache! CacheKey: {CacheKey} - Statement: {Statement}", statementCacheKey, JsonConvert.SerializeObject(statement));
        }

        return Result.Success(statement ?? new CashFlowStatementReadModel(request.CompanyAccountId, Statements: [], 0));
    }

    public async Task<Result> ValidateAsync(CashFlowStatementByOperationQuery request)
    {
        if (request == null)
            return CashFlowStatementErrors.RequiredRequest;

        if (request.CompanyAccountId == Guid.Empty)
            return CashFlowStatementErrors.WithInvalidCompanyAccountId(request.CompanyAccountId);

        var validationResults
            = await new AutoValidator()
                .Build<CashFlowStatementByOperationQuery>()
                .With(ctx => ctx.CompanyAccountId, async value => await _service.BankAccountExistsAsync(value), $"{nameof(request.CompanyAccountId)}: {request.CompanyAccountId} not found! ")
                .With(ctx => ctx.OperationType, value => OperationType.All != value, "Query only allows Inflow or Outflow transactions!")
                .ValidateAsync(request);

        if (!validationResults.IsValid)
        {
            string errorMessage = string.Join(Environment.NewLine, validationResults.Errors.Select(e => $"-> {e.Key} => {e.Value}"));

            return CashFlowStatementErrors.FailedValidationRules(errorMessage);
        }

        return Result.Success();
    }
}