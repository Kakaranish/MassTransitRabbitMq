namespace MassTransitRabbitMq.Configuration.MassTransit;

public record ConsumerMessageRetryConfiguration(
    int RetriesCount,
    TimeSpan Interval
)
{
    public static ConsumerMessageRetryConfiguration GetDefault()
    {
        return new ConsumerMessageRetryConfiguration(0, TimeSpan.Zero);
    }
}