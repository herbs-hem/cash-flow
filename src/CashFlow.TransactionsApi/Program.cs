using MassTransit;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using CashFlow.Application.CommandHandlers;
using CashFlow.Application.Extensions;
using CashFlow.Application.Services;
using CashFlow.Infrastructure.Messaging;
using CashFlow.Infrastructure.Persistence.Cache;
using CashFlow.Infrastructure.Persistence.NoSql;
using CashFlow.Infrastructure.Persistence.Sql;
using CashFlow.Infrastructure.Settings;
using CashFlow.TransactionsApi.Extensions;
using CashFlow.TransactionsApi.Services;
using RabbitMqSettings = CashFlow.Infrastructure.Settings.RabbitMQ;
using MongoDbSettings = CashFlow.Infrastructure.Settings.MongoDB;
using MediatR;
using CashFlow.Application.Commands;

var builder = WebApplication.CreateBuilder(args);

// Configurações
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(RabbitMqSettings.SectionName));
builder.Services.Configure<MySqlDB>(builder.Configuration.GetSection(MySqlDB.SectionName));
builder.Services.Configure<Redis>(builder.Configuration.GetSection(Redis.SectionName));
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection(MongoDbSettings.SectionName));
builder.Services.AddSingleton<IAppSettings, AppSettings>();

// 2. Configuração do MediatR (apenas command handlers)
builder.Services.AddMediatR(cfg =>
{
    // REMARK: O uso desta maneira registrará todos os IRequestHandler<> quanto os INotificationHandler<> que estão no Assembly
    //         No caso abaixo, isto irá procurar no Assembly da camada Application e registrar tudo que ele encontrar.
    //cfg.RegisterServicesFromAssembly(typeof(InFlowRequestCommandHandler).Assembly);
    //cfg.RegisterServicesFromAssembly(typeof(OutFlowRequestCommandHandler).Assembly);

    // Da forma abaixo, iremos registrar apenas o Core do MediatR
    cfg.RegisterServicesFromAssembly(typeof(IMediator).Assembly);
});

// Registrado o Core do MediatR, podemos registrar manualmente os IRequestHandler e/ou INotificationHandler que desejamos ao Entrypoint em questão.
builder.Services
    .AddTransient(typeof(IRequestHandler<InFlowCommand, Guid>), typeof(InFlowRequestCommandHandler))
    .AddTransient(typeof(IRequestHandler<OutFlowCommand, Guid>), typeof(OutFlowRequestCommandHandler));

var serviceProvider = builder.Services.BuildServiceProvider();

// Databases
// Sql (MySql - Transaction database)
builder.Services.AddSqlPersistence();

// NoSql (MongoDb - EventStore - Event Sourcing)
builder.Services.AddNoSqlPersistence(serviceProvider.GetRequiredService<IAppSettings>());

// Cache (Redis - EventState - Snapshot)
builder.Services.AddCachePersistence(builder.Configuration);

// MassTransit Configuration
builder.Services.AddMassTransitBus(serviceProvider.GetRequiredService<IAppSettings>());

// CashFlow.Application services
builder.Services
    .AddScoped<ITransactionsService, TransactionsService>()
    .AddTransient<ICashManagementService, CashManagementService>();

builder.Services.AddSingleton<IPublisherFactory>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<PublisherFactory>>();
    var appSettings = provider.GetRequiredService<IAppSettings>();
    var bus = provider.GetRequiredService<IBus>(); // Changed from IPublishEndpoint to IBus

    try
    {
        return new PublisherFactory(logger, appSettings, bus);
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Failed to create PublisherFactory");
        throw;
    }
});

// Configuração global para evitar problemas com DateTime
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
BsonSerializer.RegisterSerializer(new DateTimeSerializer(DateTimeKind.Utc, BsonType.String));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PocCQRS API", Version = "v1" });
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
app.MapDailyCashManagementEndpoints();
app.MapSettingsEndpoints();

await app.RunAsync();