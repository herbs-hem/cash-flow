using CashFlow.Api.Application.Commands;
using CashFlow.Api.EntryPoints.Endpoints.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Api.EntryPoints.Endpoints.Transactions;

public static class CashFlowTransactionEndpoints
{
    public static void MapCashFlowTransactionEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/account/outflow/{cashierId}",
            async ([FromRoute] string cashierId, [FromBody] OutflowRequest request, IMediator mediator) =>
            {
                var command = new OutflowCommand(request.Amount, Guid.Parse(cashierId), request.Description);

                Results.Ok(await mediator.Send(command));
            })
            .WithName("Withdrawl")
            .WithSummary("Withdrawl money from cashier")
            .WithDescription("Debit an amount value from the cashier")
            .WithTags("CashFlowTransaction")
            .WithOpenApi();

        app.MapPatch("/api/v1/account/inflow/{cashierId}",
            async ([FromRoute] string cashierId, [FromBody] InflowRequest request, IMediator mediator) =>
            {
                var command = new InflowCommand(request.Amount, Guid.Parse(cashierId), request.Description);

                Results.Ok(await mediator.Send(command));
            })
            .WithName("Deposit")
            .WithSummary("Deposit money into cashier")
            .WithDescription("Credit an amount value into the cashier")
            .WithTags("CashFlowTransaction")
            .WithOpenApi();
    }
}