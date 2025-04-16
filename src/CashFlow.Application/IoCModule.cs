using CashFlow.Application.EventHandlers;
using CashFlow.Domain.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CashFlow.Application;

public static class IoCModule
{
    /// <summary>
    /// Register Event Handlers
    /// </summary>
    /// <param name="builder"></param>
    /// <returns>Returns <see cref="IHostApplicationBuilder"/></returns>
    public static IHostApplicationBuilder AddEventHandler(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IEventHandler<InFlowProcessedEvent, Guid>, InFlowProcessedEventHandler>();
        builder.Services.AddScoped<IEventHandler<OutFlowProcessedEvent, Guid>, OutFlowProcessedEventHandler>();
        
        return builder;
    }
    
}
