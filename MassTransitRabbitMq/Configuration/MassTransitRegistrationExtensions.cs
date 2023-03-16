using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransitRabbitMq.Configuration.AppSettingsConfig;
using MassTransitRabbitMq.Configuration.MassTransit;
using MassTransitRabbitMq.Filters;
using MassTransitRabbitMq.Messaging;
using MassTransitRabbitMq.Persistence;

namespace MassTransitRabbitMq.Configuration;

public static class MassTransitRegistrationExtensions
{
    public static void RegisterMassTransit(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IConsumerRetryConfigurationProvider, ConsumerRetryConfigurationProvider>();
        
        var rabbitMqConfig = new RabbitMqConfiguration();
        builder.Configuration.GetSection("RabbitMq").Bind(rabbitMqConfig);
        builder.Services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.AddEntityFrameworkOutbox<AppDbContext>(outboxConfigurator =>
            {
                outboxConfigurator.UseBusOutbox();
                outboxConfigurator.LockStatementProvider = new PostgresLockStatementProvider();
            });
            
            busConfigurator.AddConsumer<AddPersonIntegrationCommandHandler>();

            busConfigurator.UsingRabbitMq((rabbitContext, rabbitConfigurator) =>
            {
                rabbitConfigurator.Host(rabbitMqConfig.Host, (ushort) rabbitMqConfig.Port, rabbitMqConfig.VirtualHost, 
                    hostConfigurator =>
                {
                    hostConfigurator.Username(rabbitMqConfig.Username);
                    hostConfigurator.Password(rabbitMqConfig.Password);
                });
        
                rabbitConfigurator.ReceiveEndpoint(Endpoints.AppWorker, endpointConfigurator =>
                {
                    endpointConfigurator.UseConsumeFilter(typeof(CustomRetryFilter<>), rabbitContext);
                    endpointConfigurator.UseEntityFrameworkOutbox<AppDbContext>(rabbitContext);
                    
                    endpointConfigurator.UseConsumeFilter(typeof(ExceptionAuditFilter<>), rabbitContext);
            
                    endpointConfigurator.ConfigureConsumer<AddPersonIntegrationCommandHandler>(rabbitContext,
                        consumerConfigurator =>
                        {
                            consumerConfigurator.UseFilter(new HandlerTypeForwarderFilter<AddPersonIntegrationCommandHandler>());
                        });
                });
            });
        });
    }
}