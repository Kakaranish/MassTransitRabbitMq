using MassTransit;
using MassTransitRabbitMq.Messaging;
using MassTransitRabbitMq.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace MassTransitRabbitMq.Controllers;

[ApiController]
[Route("app")]
public class ApplicationController : ControllerBase
{
    private readonly ILogger<ApplicationController> _logger;
    private readonly ISendEndpointProvider _sendEndpointProvider;
    private readonly AppDbContext _appDbContext;

    public ApplicationController(ILogger<ApplicationController> logger, 
        ISendEndpointProvider sendEndpointProvider,
        AppDbContext appDbContext)
    {
        _logger = logger;
        _sendEndpointProvider = sendEndpointProvider;
        _appDbContext = appDbContext;
    }

    [HttpPost("person")]
    public async Task AddPerson([FromBody] AddPersonDto addPersonDto)
    {
        var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{Endpoints.AppWorker}"));
        var addPersonIntegrationCmd = new AddPersonIntegrationCommand
        {
            FirstName = addPersonDto.FirstName,
            LastName = addPersonDto.LastName,
            DateOfBirth = addPersonDto.DateOfBirth
        };
        
        await sendEndpoint.Send(addPersonIntegrationCmd);

        await _appDbContext.SaveChangesAsync();
        
        _logger.LogInformation("Published message");
    }

    public class AddPersonDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateOnly DateOfBirth { get; set; }
    }
}