namespace MassTransitRabbitMq.Messaging;

public class AddPersonIntegrationCommand
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateOnly DateOfBirth { get; set; }
}