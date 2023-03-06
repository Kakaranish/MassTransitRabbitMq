using MassTransit;
using MassTransitRabbitMq.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace MassTransitRabbitMq.Controllers;

[ApiController]
[Route("app")]
public class ApplicationController : ControllerBase
{
    private readonly ILogger<ApplicationController> _logger;
    private readonly ISendEndpointProvider _sendEndpointProvider;

    public ApplicationController(ILogger<ApplicationController> logger, 
        ISendEndpointProvider sendEndpointProvider)
    {
        _logger = logger;
        _sendEndpointProvider = sendEndpointProvider;
    }

    [HttpPost("message")]
    public async Task AddMessage()
    {
        var command = new SaySomethingIntegrationCommand
        {
            AddedAt = DateTime.UtcNow,
            ContentToSay = "Hello world from application controller"
        };
        
        var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{Endpoints.AppWorker}"));
        await sendEndpoint.Send(command);
        
        _logger.LogInformation("Published message");
    }
}