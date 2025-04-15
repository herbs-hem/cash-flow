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
        app.MapGet("/api/v1/cashflow/statement/{companyAccountId}",
            async ([FromRoute] string companyAccountId, IMediator mediator) =>
            {
                var query = new CashFlowStatementAllQuery(Guid.Parse(companyAccountId));
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

        app.MapGet("/api/v1/cashflow/statement/{companyAccountId}/{days}",
            async ([FromRoute] string companyAccountId, int days, IMediator mediator) =>
            {
                var query = new CashFlowStatementByDaysQuery(Guid.Parse(companyAccountId), days);
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

        app.MapGet("/api/v1/cashflow/statement/{companyAccountId}/inflow",
            async ([FromRoute] string companyAccountId, IMediator mediator) =>
            {
                var query = new CashFlowStatementByOperationQuery(Guid.Parse(companyAccountId), OperationType: OperationType.Inflow);
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

        app.MapGet("/api/v1/cashflow/statement/{companyAccountId}/inflow/{days}",
            async ([FromRoute] string companyAccountId, int days, IMediator mediator) =>
            {
                var query = new CashFlowStatementByDaysAndOperationQuery(Guid.Parse(companyAccountId), Days: days, OperationType: OperationType.Inflow);
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

        app.MapGet("/api/v1/cashflow/statement/{companyAccountId}/outflow",
            async ([FromRoute] string companyAccountId, IMediator mediator) =>
            {
                var query = new CashFlowStatementByOperationQuery(Guid.Parse(companyAccountId), OperationType: OperationType.Outflow);
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

        app.MapGet("/api/v1/cashflow/statement/{companyAccountId}/outflow/{days}",
            async ([FromRoute] string companyAccountId, int days, IMediator mediator) =>
            {
                var query = new CashFlowStatementByDaysAndOperationQuery(Guid.Parse(companyAccountId), Days: days, OperationType: OperationType.Outflow);
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