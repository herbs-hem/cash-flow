using CashFlow.Application;
using CashFlow.Application.CommandHandlers;
using CashFlow.Application.Consumer;
using CashFlow.Application.EventHandlers;
using CashFlow.Application.Extensions;
using CashFlow.Application.Mapping;
using CashFlow.Application.Services;
using CashFlow.Domain.Events;
using CashFlow.Infrastructure.Messaging;
using CashFlow.Infrastructure.Persistence.Cache;
using CashFlow.Infrastructure.Persistence.NoSql;
using CashFlow.Infrastructure.Persistence.Sql;
using CashFlow.Infrastructure.Persistence.Sql.Interfaces;
using CashFlow.Infrastructure.Persistence.Sql.Repository;
using CashFlow.Infrastructure.Settings;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using RabbitMqSettings = CashFlow.Infrastructure.Settings.RabbitMQ;
using MongoDbSettings = CashFlow.Infrastructure.Settings.MongoDB;

namespace CashFlow.TransactionsExecutor;

public class Program
{
    private class Constants
    {
        public static string ApplicationName = "CashFlow.TransactionsExecutor";
    }
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        
        builder.Configuration.Sources.Clear(); 
        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();
        
        // Configurações
        builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(RabbitMqSettings.SectionName));
        builder.Services.Configure<MySqlDB>(builder.Configuration.GetSection(MySqlDB.SectionName));
        builder.Services.Configure<Redis>(builder.Configuration.GetSection(Redis.SectionName));
        builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection(MongoDbSettings.SectionName));
        builder.Services.AddSingleton<IAppSettings, AppSettings>();
        
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

        // Registrando todos os handlers
        builder.AddEventHandler();
        
        // Registrando o consumer genérico
        builder.Services.AddScoped(typeof(IConsumer<>), typeof(EventConsumer<>));
        builder.Services.AddScoped(typeof(IConsumer<>), typeof(DeadLetterEventConsumer<>));

        // Registrando o CommandHandler usado pelo Consumer
        builder.Services.AddMediatR(cfg =>
        { 
            cfg.RegisterServicesFromAssembly(typeof(ReprocessConsolidatedReportsCommandHandler).Assembly);
        });
        
        // CashFlow.Application services
        builder.Services
            .AddTransient<ITransactionsService, TransactionsService>()
            .AddTransient<IReportingService, ReportingService>();

        builder.Services.AddAutoMapper(typeof(ReportProfile).Assembly);
        
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

        var host = builder.Build();

        await host.RunAsync();
    }
}