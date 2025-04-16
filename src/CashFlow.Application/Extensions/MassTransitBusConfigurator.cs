using CashFlow.Application.Consumer;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using CashFlow.Infrastructure.Settings;
using RabbitMQ.Client;
using Microsoft.Extensions.Hosting;
using RabbitMqSettings = CashFlow.Infrastructure.Settings.RabbitMQ;
using Microsoft.Extensions.Configuration;
using MassTransit.Configuration;
using static Org.BouncyCastle.Math.EC.ECCurve;
using Polly;

namespace CashFlow.Application.Extensions;

public static class MassTransitBusConfigurator
{

    public static IHostApplicationBuilder AddMassTransitRabbitMqPublisher(this IHostApplicationBuilder builder)
    {
        var rabbitMqSettings = GetRabbitMqSettings(builder.Configuration);

        builder.Services.AddMassTransit(config =>
        {
            config.UsingRabbitMq((_, cfg) =>
            {
                cfg.Host(rabbitMqSettings.Host, rabbitMqSettings.VirtualHost, hostConfigurator =>
                {
                    hostConfigurator.Username(rabbitMqSettings.Username);
                    hostConfigurator.Password(rabbitMqSettings.Password);
                });
            });
        });

        return builder;
    }

    public static IHostApplicationBuilder AddMassTransitRabbitMqSubscribers(this IHostApplicationBuilder builder)
    {
        return AddMassTransitRabbitMq(builder);
    }

    private static IHostApplicationBuilder AddMassTransitRabbitMq(IHostApplicationBuilder builder)
    {
        var rabbitMqSettings = GetRabbitMqSettings(builder.Configuration);

        builder.Services.AddMassTransit(config =>
        {
            AddMassTransitRabbitMqConsumers(rabbitMqSettings, config);

            config.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqSettings.Host, rabbitMqSettings.VirtualHost, hostConfigurator =>
                {
                    hostConfigurator.Username(rabbitMqSettings.Username);
                    hostConfigurator.Password(rabbitMqSettings.Password);
                });

                UseMassTransitRabbitMqConsumers(rabbitMqSettings, context, cfg);
            });

        });

        return builder;
    }

    private static void AddMassTransitRabbitMqConsumers(RabbitMqSettings.MainSettings rabbitMqSettings, IBusRegistrationConfigurator busRegistrationConfigurator)
    {
        foreach (var queue in rabbitMqSettings.Queues)
        {
            Type consumerType = GetEventConsumerBy(queue.Value.Name);

            busRegistrationConfigurator.AddConsumer(consumerType);

            Type dlqConsumerType = GetDlqEventConsumerBy(queue.Value.Name);

            busRegistrationConfigurator.AddConsumer(dlqConsumerType);
        }
    }

    private static void UseMassTransitRabbitMqConsumers(RabbitMqSettings.MainSettings rabbitMqSettings, IBusRegistrationContext busRegistrationContext, IRabbitMqBusFactoryConfigurator rabbitMqBusFactoryConfigurator)
    {
        foreach (var queue in rabbitMqSettings.Queues)
        {
            Type consumerType = GetEventConsumerBy(queue.Value.Name);

            rabbitMqBusFactoryConfigurator.ReceiveEndpoint(queue.Value.Name, e =>
            {
                // Desabilita a criação da fila de erro padrão
                e.DiscardFaultedMessages();

                e.SetQueueArgument("x-dead-letter-exchange", queue.Value.DLQ.Exchange);
                e.SetQueueArgument("x-dead-letter-routing-key", queue.Value.DLQ.Queue);

                e.ConfigureConsumer(busRegistrationContext, consumerType);
            });

            rabbitMqBusFactoryConfigurator.ReceiveEndpoint(queue.Value.DLQ.Queue, e =>
            {

                e.Bind(queue.Value.DLQ.Exchange, x =>
                {
                    x.RoutingKey = queue.Value.DLQ.Queue;
                    x.ExchangeType = ExchangeType.Direct;
                });

                // Tratar mensagens na DLQ aqui
                // Define o consumidor para a DLQ
                var consumerType = GetDlqEventConsumerBy(queue.Value.Name);
                e.ConfigureConsumer(busRegistrationContext, consumerType);
            });
        }
    }

    private static RabbitMqSettings.MainSettings GetRabbitMqSettings(IConfiguration configuration)
    {
        var rabbitMqSettigns = configuration.GetSection(RabbitMqSettings.SectionName).Get<RabbitMqSettings>();
        var mainRabbitMqSettings = rabbitMqSettigns?.Main ?? new RabbitMqSettings.MainSettings();

        return mainRabbitMqSettings;
    }

    public static IServiceCollection AddMassTransitBus(this IServiceCollection services, IAppSettings appSettings)
    {
        var config = appSettings.RabbitMQSettings.Main;

        services.AddMassTransit(registrationConfigurator =>
        {
            foreach (var queue in config.Queues)
            {
                Type consumerType = GetEventConsumerBy(queue.Value.Name);

                registrationConfigurator.AddConsumer(consumerType);

                Type dlqConsumerType = GetDlqEventConsumerBy(queue.Value.Name);

                registrationConfigurator.AddConsumer(dlqConsumerType);
            }

            registrationConfigurator.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(config.Host, config.VirtualHost, hostConfigurator =>
                {
                    hostConfigurator.Username(config.Username);
                    hostConfigurator.Password(config.Password);
                });

                foreach (var queue in config.Queues)
                {
                    Type consumerType = GetEventConsumerBy(queue.Value.Name);

                    cfg.ReceiveEndpoint(queue.Value.Name, e =>
                    {
                        // Desabilita a criação da fila de erro padrão
                        e.DiscardFaultedMessages();

                        e.SetQueueArgument("x-dead-letter-exchange", queue.Value.DLQ.Exchange);
                        e.SetQueueArgument("x-dead-letter-routing-key", queue.Value.DLQ.Queue);

                        e.ConfigureConsumer(context, consumerType);
                    });

                    cfg.ReceiveEndpoint(queue.Value.DLQ.Queue, e =>
                    {
                        
                        e.Bind(queue.Value.DLQ.Exchange, x =>
                        {                            
                            x.RoutingKey = queue.Value.DLQ.Queue;
                            x.ExchangeType = ExchangeType.Direct;
                        });

                        // Tratar mensagens na DLQ aqui
                        // Define o consumidor para a DLQ
                        var consumerType = GetDlqEventConsumerBy(queue.Value.Name);
                        e.ConfigureConsumer(context, consumerType);
                    });
                }
            });
        });

        services.AddMassTransitHostedService();
        return services;
    }

    private static Type GetEventConsumerBy(string queueName)
    {
        var eventType = GetConsumerTypeByQueueName(queueName);

        return typeof(EventConsumer<>).MakeGenericType(eventType);
    }

    private static Type GetDlqEventConsumerBy(string queueName)
    {
        var eventType = GetConsumerTypeByQueueName(queueName);

        return typeof(DeadLetterEventConsumer<>).MakeGenericType(eventType);
    }

    private static Type GetConsumerTypeByQueueName(string queueName)
    {
        var eventType = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.Name == queueName);

        if (eventType == null)
        {
            throw new InvalidOperationException($"Tipo de evento '{queueName}' não encontrado.");
        }

        return eventType;
    }
}