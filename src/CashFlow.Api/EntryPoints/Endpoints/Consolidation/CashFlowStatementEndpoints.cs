using MediatR;
using Microsoft.AspNetCore.Mvc;
using CashFlow.Api.Application.Queries.Statement;
using CashFlow.Api.Domain.Enums;
using CashFlow.Api.EntryPoints.Endpoints.Extensions;

namespace CashFlow.Api.EntryPoints.Endpoints.Consolidation;

public static class CashFlowStatementEndpoints
{
    public static void MapBankAccountStatementEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/account/statement/{bankAccountId}",
            async ([FromRoute] string bankAccountId, IMediator mediator) =>
            {
                var query = new CashFlowStatementAllQuery(Guid.Parse(bankAccountId));
                var result = await mediator.Send(query);

                if (result is null)
                    return Results.NotFound("Bank account not found!");

                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
            })
            .WithName("GetBankStatement")
            .WithSummary("Bank Statement of last 15 days")
            .WithDescription("Retrieve the bank statement of last 15 days")
            .WithTags("BankStatement")
            .WithOpenApi();

        app.MapGet("/api/v1/account/statement/{bankAccountId}/{days}",
            async ([FromRoute] string bankAccountId, int days, IMediator mediator) =>
            {
                var query = new CashFlowStatementByDaysQuery(Guid.Parse(bankAccountId), days);
                var result = await mediator.Send(query);

                if (result is null)
                    return Results.NotFound("Bank account not found!");

                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
            })
            .WithName("GetBankStatementByPeriod")
            .WithSummary("Bank Statement of last period of days")
            .WithDescription("Retrieve the bank statement of last period days")
            .WithTags("BankStatement")
            .WithOpenApi();

        app.MapGet("/api/v1/account/statement/{bankAccountId}/inflow",
            async ([FromRoute] string bankAccountId, IMediator mediator) =>
            {
                var query = new CashFlowStatementByOperationQuery(Guid.Parse(bankAccountId), OperationType: OperationType.Inflow);
                var result = await mediator.Send(query);

                if (result is null)
                    return Results.NotFound("Bank account not found!");

                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
            })
            .WithName("GetInflowBankStatement")
            .WithSummary("Inflow Bank Statement of last 15 of days")
            .WithDescription("Retrieve the inflow bank statement of 15 period days")
            .WithTags("BankStatement")
            .WithOpenApi();

        app.MapGet("/api/v1/account/statement/{bankAccountId}/inflow/{days}",
            async ([FromRoute] string bankAccountId, int days, IMediator mediator) =>
            {
                var query = new CashFlowStatementByDaysAndOperationQuery(Guid.Parse(bankAccountId), Days: days, OperationType: OperationType.Inflow);
                var result = await mediator.Send(query);

                if (result is null)
                    return Results.NotFound("Bank account not found!");

                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
            })
            .WithName("GetInflowBankStatementByPeriod")
            .WithSummary("Inflow Bank Statement of last period of days")
            .WithDescription("Retrieve the inflow bank statement of last period days")
            .WithTags("BankStatement")
            .WithOpenApi();

        app.MapGet("/api/v1/account/statement/{bankAccountId}/outflow",
            async ([FromRoute] string bankAccountId, IMediator mediator) =>
            {
                var query = new CashFlowStatementByOperationQuery(Guid.Parse(bankAccountId), OperationType: OperationType.Outflow);
                var result = await mediator.Send(query);

                if (result is null)
                    return Results.NotFound("Bank account not found!");

                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
            })
            .WithName("GetOutflowBankStatement")
            .WithSummary("Outflow Bank Statement of last 15 of days")
            .WithDescription("Retrieve the outflow bank statement of last 15 days")
            .WithTags("BankStatement")
            .WithOpenApi();

        app.MapGet("/api/v1/account/statement/{bankAccountId}/outflow/{days}",
            async ([FromRoute] string bankAccountId, int days, IMediator mediator) =>
            {
                var query = new CashFlowStatementByDaysAndOperationQuery(Guid.Parse(bankAccountId), Days: days, OperationType: OperationType.Outflow);
                var result = await mediator.Send(query);

                if (result is null)
                    return Results.NotFound("Bank account not found!");

                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
            })
            .WithName("GetOutflowBankStatementByPeriod")
            .WithSummary("Outflow Bank Statement of last period of days")
            .WithDescription("Retrieve the outflow bank statement of last period days")
            .WithTags("BankStatement")
            .WithOpenApi();
    }
}