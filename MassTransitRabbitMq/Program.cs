using MassTransit;
using MassTransitRabbitMq.Configuration;
using MassTransitRabbitMq.Filters;
using MassTransitRabbitMq.Messaging;
using MassTransitRabbitMq.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
var rabbitMqConfig = new RabbitMqConfiguration();
builder.Configuration.GetSection("RabbitMq").Bind(rabbitMqConfig);

var postgresConnStr = builder.Configuration.GetValue<string>("Database:ConnectionString");
builder.Services.AddDbContext<AppDbContext>(optionsBuilder =>
{
    optionsBuilder.UseNpgsql(postgresConnStr);
});

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<AddPersonIntegrationCommandHandler>();
    configurator.AddEntityFrameworkOutbox<AppDbContext>();
    
    configurator.UsingRabbitMq((rabbitContext, rabbitConfigurator) =>
    {
        rabbitConfigurator.Host(rabbitMqConfig.Host, (ushort) rabbitMqConfig.Port, rabbitMqConfig.VirtualHost, hostConfigurator =>
        {
            hostConfigurator.Username(rabbitMqConfig.Username);
            hostConfigurator.Password(rabbitMqConfig.Password);
        });
        
        rabbitConfigurator.ReceiveEndpoint(Endpoints.AppWorker, endpointConfigurator =>
        {
            endpointConfigurator.UseEntityFrameworkOutbox<AppDbContext>(rabbitContext);
            
            endpointConfigurator.UseConsumeFilter(typeof(ExceptionAuditFilter<>), rabbitContext);
            
            endpointConfigurator.ConfigureConsumer<AddPersonIntegrationCommandHandler>(rabbitContext);
            endpointConfigurator.UseRetry(r =>
            {
                r.Handle<Exception>();
                r.Interval(2, TimeSpan.FromSeconds(5));
            });
        });
    });
});

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.Run();