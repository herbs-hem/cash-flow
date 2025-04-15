using MediatR;
using Microsoft.AspNetCore.Mvc;
using CashFlow.Api.Application.Queries.Balance;
using CashFlow.Api.EntryPoint.EndPoints.Extensions;

namespace CashFlow.Api.EntryPoints.Endpoints.Consolidation;

public static class BankAccountBalanceEndpoints
{
    public static void MapBankAccountBalanceEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/account/balance/{bankAccountId}",
            async ([FromRoute] string bankAccountId, IMediator mediator) =>
            {
                var query = new BankBalanceByBankAccountIdQuery(Guid.Parse(bankAccountId));
                var result = await mediator.Send(query);

                if (result is null)
                    return Results.NotFound("Bank account not found!");

                return result.IsSuccess ? Results.Ok(new { result.Value!.BankAccountId, result.Value!.Balance }) : result.ToProblemDetails();
            })
            .WithName("GetBankBalannce")
            .WithSummary("Bank Balance")
            .WithDescription("Retrieve the bank balance of a bank account number")
            .WithTags("BankBalance")
            .WithOpenApi();
    }
}