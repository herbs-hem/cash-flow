using MediatR;
using Microsoft.AspNetCore.Mvc;
using CashFlow.Api.Application.Queries.Balance;
using CashFlow.Api.EntryPoints.Endpoints.Extensions;

namespace CashFlow.Api.EntryPoints.Endpoints.Consolidation;

public static class CashFlowBalanceEndpoints
{
    public static void MapCashFlowBalanceEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/cashflow/balance/{companyAccountId}",
            async ([FromRoute] string companyAccountId, IMediator mediator) =>
            {
                var query = new CashFlowBalanceByCompanyAccountIdQuery(Guid.Parse(companyAccountId));
                var result = await mediator.Send(query);

                if (result is null)
                    return Results.NotFound("Cash Flow Balance not found!");

                return result.IsSuccess ? Results.Ok(new { result.Value!.CompanyAccountId, result.Value!.Balance }) : result.ToProblemDetails();
            })
            .WithName("GetBalannce")
            .WithSummary("Cash Flow Balance")
            .WithDescription("Retrieve the cash flow balance of a cashier identifier")
            .WithTags("CashFlowBalance")
            .WithOpenApi();
    }
}