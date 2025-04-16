using Microsoft.Extensions.Configuration;

namespace CashFlow.Infrastructure.Messaging;

public class QueueConfig
{
    private readonly IConfiguration _configuration;

    public QueueConfig(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetQueueName<T>()
    {
        var eventTypeName = typeof(T).Name; // Obtém o nome do evento, ex: "OrderCreatedEvent"
        
        var queuesSection = _configuration.GetSection("Broker:Main:Queues");
        if (!queuesSection.Exists())
        {
            throw new InvalidOperationException("Configuração de filas não encontrada no appsettings.json.");
        }

        foreach (var queue in queuesSection.GetChildren())
        {
            var queueName = queue.GetValue<string>("Name");
            if (queueName == eventTypeName) // Verifica se o nome da fila corresponde ao nome do evento
            {
                return queue.Key; // Retorna a chave do JSON (ex: "OrderCreatedQueue")
            }
        }

        throw new InvalidOperationException($"Nenhuma fila configurada para o evento {eventTypeName}.");
    }
}