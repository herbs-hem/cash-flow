using CashFlow.Infrastructure.Settings;

namespace CashFlow.ConsolidationApi.Extensions;

public static class SettingsEndpoints
{
    public static void MapSettingsEndpoints(this WebApplication app)
    {
        app.MapPost("/settings/reload", async (IAppSettings settings) => 
        {
            await settings.ReloadAsync();
            return Results.Ok("Configurações recarregadas");
        });
    }
}