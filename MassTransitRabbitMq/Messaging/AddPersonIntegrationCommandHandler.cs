using MassTransit;
using MassTransitRabbitMq.Persistence;
using MassTransitRabbitMq.Persistence.Entities;

namespace MassTransitRabbitMq.Messaging;

public class AddPersonIntegrationCommandHandler : IConsumer<AddPersonIntegrationCommand>
{
    private readonly AppDbContext _appDbContext;
    private readonly ILogger<AddPersonIntegrationCommandHandler> _logger;

    public AddPersonIntegrationCommandHandler(AppDbContext appDbContext, 
        ILogger<AddPersonIntegrationCommandHandler> logger)
    {
        _appDbContext = appDbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AddPersonIntegrationCommand> context)
    {
        _logger.LogInformation("Handling message with id = {MessageId}", context.MessageId);

        var transaction = await _appDbContext.Database.BeginTransactionAsync(context.CancellationToken);
        
        var person = new Person
        {
            FirstName = context.Message.FirstName,
            LastName = context.Message.LastName,
            DateOfBirth = context.Message.DateOfBirth
        };
        
        _appDbContext.Add(person);
        await _appDbContext.SaveChangesAsync(context.CancellationToken);

        await transaction.CommitAsync(context.CancellationToken);
        
        _logger.LogInformation("Created user with id = {UserId}", person.Id);
    }
}