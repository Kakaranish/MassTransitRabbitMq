namespace MassTransitRabbitMq.Messaging;

public class SaySomethingIntegrationCommand
{
    public DateTime AddedAt { get; set; }
    public string ContentToSay { get; set; } = null!;
}