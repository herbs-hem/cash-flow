using CashFlow.Domain.Abstractions;
using CashFlow.TransactionsApi.Models.Requests;
using CashFlow.TransactionsApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.TransactionsApi.Extensions;

public static class DailyCashManagementEndpoints
{
    public static void MapDailyCashManagementEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/outflow/", ([FromBody] OutFlowRequest request, [FromServices] ICashManagementService service)
            => service.CreateOutFlowRequest(request).Result.ToIResult());
        
        app.MapPost("/api/v1/inflow/", ([FromBody] InFlowRequest request, [FromServices] ICashManagementService service)
            => service.CreateInFlowRequest(request).Result.ToIResult());
        
        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                var result = HttpResult<object>.InternalServerError(new Error(ex.Message));
                await context.Response.WriteAsJsonAsync(result);
            }
        });
    }
}