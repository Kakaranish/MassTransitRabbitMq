namespace MassTransitRabbitMq.Configuration.AppSettingsConfig;

public class RabbitMqConfiguration
{
    public string Host { get; set; } = null!;
    public int Port { get; set; } 
    public string VirtualHost { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}