namespace MassTransitRabbitMq.Configuration.MassTransit;

public interface IConsumerRetryConfigurationProvider
{
    void AddConfiguration<THandler>(int retriesCount, TimeSpan interval);
    ConsumerMessageRetryConfiguration GetForHandler(Type handlerType);
}

public class ConsumerRetryConfigurationProvider : IConsumerRetryConfigurationProvider
{
    private readonly Dictionary<Type, ConsumerMessageRetryConfiguration> _configDict = new();

    public void AddConfiguration<THandler>(int retriesCount, TimeSpan interval)
    {
        _configDict.TryAdd(typeof(THandler), new ConsumerMessageRetryConfiguration(retriesCount, interval));
    }
    
    public ConsumerMessageRetryConfiguration GetForHandler(Type handlerType)
    {
        return _configDict.TryGetValue(handlerType, out var retryConfig)
            ? retryConfig
            : ConsumerMessageRetryConfiguration.GetDefault();
    }
}