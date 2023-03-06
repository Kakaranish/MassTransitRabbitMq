using MassTransit;
using MassTransitRabbitMq.Configuration;
using MassTransitRabbitMq.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
var rabbitMqConfig = new RabbitMqConfiguration();
builder.Configuration.GetSection("RabbitMq").Bind(rabbitMqConfig);

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<SaySomethingIntegrationCommandHandler>();
    
    configurator.UsingRabbitMq((rabbitContext, rabbitConfigurator) =>
    {
        rabbitConfigurator.Host(rabbitMqConfig.Host, (ushort) rabbitMqConfig.Port, rabbitMqConfig.VirtualHost, hostConfigurator =>
        {
            hostConfigurator.Username(rabbitMqConfig.Username);
            hostConfigurator.Password(rabbitMqConfig.Password);
        });
        rabbitConfigurator.ReceiveEndpoint(Endpoints.AppWorker, endpointConfigurator =>
        {
            endpointConfigurator.ConfigureConsumer<SaySomethingIntegrationCommandHandler>(rabbitContext);
        });
    });
});

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.Run();