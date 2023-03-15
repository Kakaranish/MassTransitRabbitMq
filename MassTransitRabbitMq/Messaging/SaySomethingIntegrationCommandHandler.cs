using MassTransit;

namespace MassTransitRabbitMq.Messaging;

public class SaySomethingIntegrationCommandHandler : IConsumer<SaySomethingIntegrationCommand>
{
    private readonly ILogger<SaySomethingIntegrationCommandHandler> _logger;

    public SaySomethingIntegrationCommandHandler(ILogger<SaySomethingIntegrationCommandHandler> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<SaySomethingIntegrationCommand> context)
    {
        throw new InvalidOperationException();
        
        _logger.LogInformation("Consumed message: {MessageId} | AddedAt: {AddedAt} | Content: {Content}",
            context.MessageId, context.Message.AddedAt, context.Message.ContentToSay);

        return Task.CompletedTask;
    }
}