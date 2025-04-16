using Microsoft.OpenApi.Models;
using CashFlow.Application.Mapping;
using CashFlow.Application.QueryHadlers;
using CashFlow.Application.Services;
using CashFlow.ConsolidationApi.Extensions;
using CashFlow.ConsolidationApi.Services;
using CashFlow.Infrastructure.Persistence.Cache;
using CashFlow.Infrastructure.Persistence.Sql;
using CashFlow.Infrastructure.Settings;
using RabbitMqSettings = CashFlow.Infrastructure.Settings.RabbitMQ;
using MongoDbSettings = CashFlow.Infrastructure.Settings.MongoDB;

var builder = WebApplication.CreateBuilder(args);

// Configurações
//builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(RabbitMqSettings.SectionName));
builder.Services.Configure<MySqlDB>(builder.Configuration.GetSection(MySqlDB.SectionName));
builder.Services.Configure<Redis>(builder.Configuration.GetSection(Redis.SectionName));
//builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection(MongoDbSettings.SectionName));
builder.Services.AddSingleton<IAppSettings, AppSettings>();

// Add services to the container.
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(GetConsolidatedDataQueryHandler).Assembly));

// Databases
// Sql (MySql - Transaction database)
builder.Services.AddSqlPersistence();

// Cache (Redis - EventState - Snapshot)
builder.Services.AddCachePersistence(builder.Configuration);

builder.Services.AddAutoMapper(typeof(ReportProfile).Assembly);

builder.Services
    .AddTransient<IConsolidationReportingService, ConsolidationReportingService>()
    .AddTransient<IReportingService, ReportingService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PocCQRS ConsolidationApi", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add redirection from root to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapReportsEndpoints();
app.MapSettingsEndpoints();

await app.RunAsync();
