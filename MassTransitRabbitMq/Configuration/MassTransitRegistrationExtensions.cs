using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransitRabbitMq.Filters;
using MassTransitRabbitMq.Messaging;
using MassTransitRabbitMq.Persistence;

namespace MassTransitRabbitMq.Configuration;

public static class MassTransitRegistrationExtensions
{
    public static void RegisterMassTransit(this WebApplicationBuilder builder)
    {
        var rabbitMqConfig = new RabbitMqConfiguration();
        builder.Configuration.GetSection("RabbitMq").Bind(rabbitMqConfig);
        builder.Services.AddMassTransit(configurator =>
        {
            configurator.AddConsumer<AddPersonIntegrationCommandHandler>();
            configurator.AddEntityFrameworkOutbox<AppDbContext>(outboxConfigurator =>
            {
                outboxConfigurator.UseBusOutbox();
                outboxConfigurator.LockStatementProvider = new PostgresLockStatementProvider();
            });
    
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
    }
}