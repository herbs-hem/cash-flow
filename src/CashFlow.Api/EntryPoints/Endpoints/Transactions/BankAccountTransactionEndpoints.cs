using CashFlow.Api.Application.Commands;
using CashFlow.Api.EntryPoints.Endpoints.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Api.EntryPoints.Endpoints.Transactions;

public static class BankAccountTransactionEndpoints
{
    public static void MapBankAccountTransactionEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/account/debit/{bankAccountId}",
            async ([FromRoute] string bankAccountId, [FromBody] OutflowRequest request, IMediator mediator) =>
            {
                var command = new OutflowCommand(request.Amount, Guid.Parse(bankAccountId), request.Description);

                Results.Ok(await mediator.Send(command));
            })
            .WithName("Withdrawl")
            .WithSummary("Withdrawl money from bank account")
            .WithDescription("Debit an amount value from the bank account")
            .WithTags("BankTransaction")
            .WithOpenApi();

        app.MapPatch("/api/v1/account/credit/{bankAccountId}",
            async ([FromRoute] string bankAccountId, [FromBody] InflowRequest request, IMediator mediator) =>
            {
                var command = new InflowCommand(request.Amount, Guid.Parse(bankAccountId), request.Description);

                Results.Ok(await mediator.Send(command));
            })
            .WithName("Deposit")
            .WithSummary("Deposit money into Bank account")
            .WithDescription("Credit an amount value into the bank account")
            .WithTags("BankTransaction")
            .WithOpenApi();
    }
}