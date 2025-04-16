using CashFlow.Domain.Abstractions;
using CashFlow.TransactionsApi.Models.Requests;

namespace CashFlow.TransactionsApi.Services;

public interface ICashManagementService
{
    public Task<HttpResult<Guid>> CreateInFlowRequest(InFlowRequest command);
    public Task<HttpResult<Guid>> CreateOutFlowRequest(OutFlowRequest command);
}