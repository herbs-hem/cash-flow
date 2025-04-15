using MediatR;
using Microsoft.AspNetCore.Mvc;
using CashFlow.Api.Application.Queries.Statement;
using CashFlow.Api.Domain.Enums;
using CashFlow.Api.EntryPoints.Endpoints.Extensions;

namespace CashFlow.Api.EntryPoints.Endpoints.Consolidation;

public static class CashFlowStatementEndpoints
{
    public static void MapCashFlowStatementEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/cashflow/statement/{cashierId}",
            async ([FromRoute] string cashierId, IMediator mediator) =>
            {
                var query = new CashFlowStatementAllQuery(Guid.Parse(cashierId));
                var result = await mediator.Send(query);

                if (result is null)
                    return Results.NotFound("Cash flow Statement not found!");

                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
            })
            .WithName("GetCashFlowStatement")
            .WithSummary("CashFlow Statement of last 15 days")
            .WithDescription("Retrieve the cash flow statement of last 15 days")
            .WithTags("CashFlowStatement")
            .WithOpenApi();

        app.MapGet("/api/v1/cashflow/statement/{cashierId}/{days}",
            async ([FromRoute] string cashierId, int days, IMediator mediator) =>
            {
                var query = new CashFlowStatementByDaysQuery(Guid.Parse(cashierId), days);
                var result = await mediator.Send(query);

                if (result is null)
                    return Results.NotFound("Cash flow statement not found!");

                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
            })
            .WithName("GetCashFlowStatementByPeriod")
            .WithSummary("Cash Flow Statement of last period of days")
            .WithDescription("Retrieve the cash flow statement of last period days")
            .WithTags("CashFlowStatement")
            .WithOpenApi();

        app.MapGet("/api/v1/cashflow/statement/{cashierId}/inflow",
            async ([FromRoute] string cashierId, IMediator mediator) =>
            {
                var query = new CashFlowStatementByOperationQuery(Guid.Parse(cashierId), OperationType: OperationType.Inflow);
                var result = await mediator.Send(query);

                if (result is null)
                    return Results.NotFound("Cash flow statement not found!");

                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
            })
            .WithName("GetInflowStatement")
            .WithSummary("Inflow Statement of last 15 of days")
            .WithDescription("Retrieve the inflow statement of 15 period days")
            .WithTags("CashFlowStatement")
            .WithOpenApi();

        app.MapGet("/api/v1/cashflow/statement/{cashierId}/inflow/{days}",
            async ([FromRoute] string cashierId, int days, IMediator mediator) =>
            {
                var query = new CashFlowStatementByDaysAndOperationQuery(Guid.Parse(cashierId), Days: days, OperationType: OperationType.Inflow);
                var result = await mediator.Send(query);

                if (result is null)
                    return Results.NotFound("Cash flow statement not found!");

                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
            })
            .WithName("GetInflowStatementByPeriod")
            .WithSummary("Inflow Statement of last period of days")
            .WithDescription("Retrieve the inflow statement of last period days")
            .WithTags("CashFlowStatement")
            .WithOpenApi();

        app.MapGet("/api/v1/cashflow/statement/{cashierId}/outflow",
            async ([FromRoute] string cashierId, IMediator mediator) =>
            {
                var query = new CashFlowStatementByOperationQuery(Guid.Parse(cashierId), OperationType: OperationType.Outflow);
                var result = await mediator.Send(query);

                if (result is null)
                    return Results.NotFound("Cash flow statement not found!");

                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
            })
            .WithName("GetOutflowStatement")
            .WithSummary("Outflow Statement of last 15 of days")
            .WithDescription("Retrieve the outflow statement of last 15 days")
            .WithTags("CashFlowStatement")
            .WithOpenApi();

        app.MapGet("/api/v1/cashflow/statement/{cashierId}/outflow/{days}",
            async ([FromRoute] string cashierId, int days, IMediator mediator) =>
            {
                var query = new CashFlowStatementByDaysAndOperationQuery(Guid.Parse(cashierId), Days: days, OperationType: OperationType.Outflow);
                var result = await mediator.Send(query);

                if (result is null)
                    return Results.NotFound("Cash flow statement not found!");

                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
            })
            .WithName("GetOutflowStatementByPeriod")
            .WithSummary("Outflow Statement of last period of days")
            .WithDescription("Retrieve the outflow statement of last period days")
            .WithTags("CashFlowStatement")
            .WithOpenApi();
    }
}