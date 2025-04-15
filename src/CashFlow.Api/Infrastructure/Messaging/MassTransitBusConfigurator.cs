using MassTransit;
using RabbitMQ.Client;
using RabbitMqSettings = CashFlow.Api.Infrastructure.Settings.RabbitMQ;

namespace CashFlow.Api.Infrastructure.Messaging;

public static class MassTransitBusConfigurator
{
    public static IServiceCollection AddMassTransitBus(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqSettings = configuration.GetSection(RabbitMqSettings.SectionName).Get<RabbitMqSettings>();
        var config = rabbitMqSettings.Main;

        services.AddMassTransit(registrationConfigurator =>
        {
            //foreach (var queue in config.Queues)
            //{
            //    Type consumerType = GetEventConsumerBy(queue.Value.Name);

            //    registrationConfigurator.AddConsumer(consumerType);

            //    Type dlqConsumerType = GetDlqEventConsumerBy(queue.Value.Name);

            //    registrationConfigurator.AddConsumer(dlqConsumerType);
            //}

            registrationConfigurator.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(config.Host, config.VirtualHost, hostConfigurator =>
                {
                    hostConfigurator.Username(config.Username);
                    hostConfigurator.Password(config.Password);
                });

                foreach (var queue in config.Queues)
                {
                    //Type consumerType = GetEventConsumerBy(queue.Value.Name);

                    cfg.ReceiveEndpoint(queue.Value.Name, e =>
                    {
                        // Desabilita a criação da fila de erro padrão
                        e.DiscardFaultedMessages();

                        e.SetQueueArgument("x-dead-letter-exchange", queue.Value.DLQ.Exchange);
                        e.SetQueueArgument("x-dead-letter-routing-key", queue.Value.DLQ.Queue);

                        //e.ConfigureConsumer(context, consumerType);
                    });

                    cfg.ReceiveEndpoint(queue.Value.DLQ.Queue, e =>
                    {
                        e.Bind(queue.Value.DLQ.Exchange, x =>
                        {
                            x.RoutingKey = queue.Value.DLQ.Queue;
                            x.ExchangeType = ExchangeType.Direct;
                        });

                        // Tratar mensagens na DLQ aqui - Define o consumidor para a DLQ
                        //var consumerType = GetDlqEventConsumerBy(queue.Value.Name);

                        //e.ConfigureConsumer(context, consumerType);
                    });
                }
            });
        });

        return services;
    }

    //private static Type GetEventConsumerBy(string queueName)
    //{
    //    var eventType = GetConsumerTypeByQueueName(queueName);

    //    return typeof(EventConsumer<>).MakeGenericType(eventType);
    //}

    //private static Type GetDlqEventConsumerBy(string queueName)
    //{
    //    var eventType = GetConsumerTypeByQueueName(queueName);

    //    return typeof(DeadLetterEventConsumer<>).MakeGenericType(eventType);
    //}

    //private static Type GetConsumerTypeByQueueName(string queueName)
    //{
    //    var eventType = AppDomain.CurrentDomain
    //        .GetAssemblies()
    //        .SelectMany(a => a.GetTypes())
    //        .FirstOrDefault(t => t.Name == queueName);

    //    if (eventType == null)
    //    {
    //        throw new InvalidOperationException($"Tipo de evento '{queueName}' não encontrado.");
    //    }

    //    return eventType;
    //}
}