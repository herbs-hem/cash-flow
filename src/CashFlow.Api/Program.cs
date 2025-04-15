using CashFlow.Api.Application.CommandHandlers;
using CashFlow.Api.Application.EventHandlers;
using CashFlow.Api.Domain.Constants;
using CashFlow.Api.Domain.Events;
using CashFlow.Api.Domain.Services;
using CashFlow.Api.EntryPoint.Consumer;
using CashFlow.Api.EntryPoints.Endpoints.Consolidation;
using CashFlow.Api.EntryPoints.Endpoints.Extensions;
using CashFlow.Api.EntryPoints.Endpoints.Middleware;
using CashFlow.Api.EntryPoints.Endpoints.Transactions;
using CashFlow.Api.Infrastructure.Cache;
using CashFlow.Api.Infrastructure.Messaging;
using CashFlow.Api.Infrastructure.Persistence.NoSql;
using CashFlow.Api.Infrastructure.Persistence.Sql;
using CashFlow.Api.Infrastructure.Settings;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using PocCQRS.Infrastructure.Messaging;
using MongoDbSettings = CashFlow.Api.Infrastructure.Settings.MongoDB;
using RabbitMqSettings = CashFlow.Api.Infrastructure.Settings.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(RabbitMqSettings.SectionName));
builder.Services.Configure<MySqlDB>(builder.Configuration.GetSection(MySqlDB.SectionName));
builder.Services.Configure<Redis>(builder.Configuration.GetSection(Redis.SectionName));
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection(MongoDbSettings.SectionName));

builder.Services.AddSingleton<IAppSettings, AppSettings>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(InflowCommandHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(OutflowCommandHandler).Assembly);
});

builder.Services.AddMassTransitBus(builder.Configuration);
builder.Services.AddSingleton<IPublisherFactory, PublisherFactory>();

builder.Services.AddSqlPersistence();
builder.Services.AddNoSqlPersistence(builder.Configuration);
builder.Services.AddCachePersistence(builder.Configuration);

// Registrando todos os handlers
builder.Services.AddScoped<IEventHandler<InflowProcessedEvent, Guid>, InflowProcessedEventHandler>();
builder.Services.AddScoped<IEventHandler<OutflowProcessedEvent, Guid>, OutflowProcessedEventHandler>();

// Registrando o consumer genérico
builder.Services.AddScoped(typeof(IConsumer<>), typeof(EventConsumer<>));
builder.Services.AddScoped(typeof(IConsumer<>), typeof(DeadLetterEventConsumer<>));

builder.Services.AddScoped<ICashFlowTransactionService, CashFlowTransactionService>();

builder.Services.AddSerilog<Program>(AppConstants.ApplicationName);
builder.Services.AddHealthChecks();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Cash Flow API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cash Flow API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseMiddleware<ExceptionHandlingExceptionMiddleware>();

app.MapCashFlowTransactionEndpoints();
app.MapCashFlowBalanceEndpoints();
app.MapCashFlowStatementEndpoints();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});

await app.RunAsync();
