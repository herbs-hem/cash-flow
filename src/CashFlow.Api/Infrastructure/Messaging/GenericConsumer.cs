using MassTransit;

namespace CashFlow.Api.Infrastructure.Messaging;

public class GenericConsumer<T> : IConsumer<T> where T : class
{
    private readonly ILogger<GenericConsumer<T>> _logger;

    public GenericConsumer(ILogger<GenericConsumer<T>> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<T> context)
    {
        _logger.LogInformation("Mensagem recebida: {Message}", context.Message);
        await Task.CompletedTask;
    }
}