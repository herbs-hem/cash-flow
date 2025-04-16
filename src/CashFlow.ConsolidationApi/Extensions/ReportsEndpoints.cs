using CashFlow.ConsolidationApi.Models.Requests;
using CashFlow.ConsolidationApi.Services;
using CashFlow.Domain.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.ConsolidationApi.Extensions;

public static class ReportsEndpoints
{
    public static void MapReportsEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/report/consolidated/", ([FromBody] ReportDailyRequest request, [FromServices] IConsolidationReportingService service)
            => service.GetConsolidatedReport(request).Result.ToIResult());
        
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