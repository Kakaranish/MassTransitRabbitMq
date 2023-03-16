using MassTransit;
using MassTransitRabbitMq.Configuration.MassTransit;

namespace MassTransitRabbitMq.Filters;

public class CustomRetryFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    private readonly ILogger<CustomRetryFilter<T>> _logger;
    private readonly IConsumerRetryConfigurationProvider _consumerRetryConfigurationProvider;

    public CustomRetryFilter(ILogger<CustomRetryFilter<T>> logger, 
        IConsumerRetryConfigurationProvider consumerRetryConfigurationProvider)
    {
        _logger = logger;
        _consumerRetryConfigurationProvider = consumerRetryConfigurationProvider;
    }

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        context.AddOrUpdatePayload(() => new HandlerContextPayload(), existing => existing);

        ConsumerMessageRetryConfiguration? retryConfig = null;
        try
        {
            await next.Send(context);
            return;
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Message={MessageId} | Main processing of message failed", context.MessageId);
            
            var handlerContext = context.GetPayload<HandlerContextPayload>();
            var handlerType = handlerContext.GetHandlerType();
            retryConfig = _consumerRetryConfigurationProvider.GetForHandler(handlerType);
            
            if (retryConfig.RetriesCount == 0)
            {
                _logger.LogError("Message={MessageId} | Processing failed with no retry policy", context.Message);
                throw;
            }
        }

        if (retryConfig is null)
        {
            throw new InvalidOperationException($"{nameof(retryConfig)} is not expected to be null");
        }

        var attemptNumber = 1;

        while (true)
        {
            try
            {
                await next.Send(context);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Message={MessageId} | Retry {AttemptNumber}/{AttemptLimit} failed",
                    context.Message, attemptNumber, retryConfig.RetriesCount);
                
                ++attemptNumber;
                
                if (attemptNumber > retryConfig.RetriesCount)
                {
                    _logger.LogError("Message={MessageId} | Retries limit exceeded", context.Message);
                    throw;
                }
            }

            await Task.Delay(retryConfig.Interval);
        }
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("custom-retry-filter");
    }
}