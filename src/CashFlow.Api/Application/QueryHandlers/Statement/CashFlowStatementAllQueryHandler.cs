using MediatR;
using CashFlow.Api.Application.Queries.Statement;
using CashFlow.Api.Application.Validators;
using CashFlow.Api.Domain.Abstractions;
using CashFlow.Api.Domain.Abstractions.Extensions;
using CashFlow.Api.Domain.Abstractions.Generic;
using CashFlow.Api.Domain.Constants;
using CashFlow.Api.Domain.Services;
using CashFlow.Api.Infrastructure.Cache.Interfaces;
using CashFlow.Api.Domain.Enums;

namespace CashFlow.Api.Application.QueryHandlers.Statement;

public class CashFlowStatementAllQueryHandler :
    IRequestHandler<CashFlowStatementAllQuery, Result<CashFlowStatementReadModel>>,
    IQueryHandler<CashFlowStatementAllQuery, Result>
{
    private readonly ICashFlowTransactionService _service;
    private readonly ICacheClient _cacheClient;
    private readonly ILogger<CashFlowStatementAllQueryHandler> _logger;

    public CashFlowStatementAllQueryHandler(
        ICashFlowTransactionService service, ICacheClient cacheClient,
        ILogger<CashFlowStatementAllQueryHandler> logger)
    {
        _service = service;
        _cacheClient = cacheClient;
        _logger = logger;
    }

    public async Task<Result<CashFlowStatementReadModel>> Handle(CashFlowStatementAllQuery request, CancellationToken cancellationToken)
    {
        var result = await ValidateAsync(request);

        if (result.IsFailure)
            return result.ToResult<CashFlowStatementReadModel>();

        var statementCacheKey = $"StatementQuery_{request.CashierId}_{DateTime.UtcNow.Date:yyyyMMdd}";

        _cacheClient.TryGetValue(statementCacheKey, out CashFlowStatementReadModel statement);

        statement ??= await _service.GetStatementAsync(request.CashierId, StatementRulesConstant.MinimumDaysLimit, OperationType.All);

        if (statement != null)
            await _cacheClient.SetAsync(statementCacheKey, statement);

        return Result.Success(statement ?? new CashFlowStatementReadModel(request.CashierId, Statements: [], 0));
    }

    public async Task<Result> ValidateAsync(CashFlowStatementAllQuery request)
    {
        if (request == null)
            return CashFlowStatementErrors.RequiredRequest;

        if (request.CashierId == Guid.Empty)
            return CashFlowStatementErrors.WithInvalidCashierId(request.CashierId);

        var validationResults
            = await new AutoValidator()
                .Build<CashFlowStatementAllQuery>()
                .With(ctx => ctx.CashierId, async value => await _service.BankAccountExistsAsync(value), $"{request.CashierId} not found!")
                .ValidateAsync(request);

        if (!validationResults.IsValid)
        {
            string errorMessage = string.Join(Environment.NewLine, validationResults.Errors.Select(e => $"{e.Key}: {e.Value}"));

            return CashFlowStatementErrors.FailedValidationRules(errorMessage);
        }

        return Result.Success();
    }
}