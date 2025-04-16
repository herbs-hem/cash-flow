using MediatR;
using CashFlow.Application.Services;
using CashFlow.Domain.Abstractions;
using CashFlow.TransactionsApi.Models.Requests;

namespace CashFlow.TransactionsApi.Services;

public class CashManagementService: ICashManagementService
{
    private readonly ITransactionsService _transactionsService;
    private readonly IMediator _mediator;
    
    public CashManagementService(ITransactionsService transactionsService, IMediator mediator)
    {
        _transactionsService = transactionsService;
        _mediator = mediator;
    }
    
    public async Task<HttpResult<Guid>> CreateInFlowRequest(InFlowRequest request)
    {
        try
        {
            if (request == null)
                return HttpResult<Guid>.BadRequest(new Error("Você precisa especificar valores válidos"));

            if (request.Amount <= 0)
                return HttpResult<Guid>.BadRequest(new Error("Valor do crédito nāo pode ser negativo ou igual a zero"));
            
            var command = new CashFlow.Application.Commands.InFlowCommand(
                request.Amount,
                request.CompanyAccountId,
                request.Description);

            var response = await _mediator.Send(command);
            return HttpResult<Guid>.Created(response);
        }
        catch (Exception ex)
        {
            return HttpResult<Guid>.InternalServerError(new Error("Erro interno no servidor"));
        }
    }

    public async Task<HttpResult<Guid>> CreateOutFlowRequest(OutFlowRequest request)
    {
        try
        {
            if (request == null)
                return HttpResult<Guid>.BadRequest(new Error("Você precisa especificar valores válidos"));

            if (request.Amount <= 0)
                return HttpResult<Guid>.BadRequest(new Error("Valor do débito nāo pode ser negativo ou igual a zero"));
            
            var command = new CashFlow.Application.Commands.OutFlowCommand(
                request.Amount,
                request.CompanyAccountId,
                request.Description);

            var response = await _mediator.Send(command);
            return HttpResult<Guid>.Created(response);

        }
        catch (Exception ex)
        {
            return HttpResult<Guid>.InternalServerError(new Error("Erro interno no servidor"));
        }
    }
}