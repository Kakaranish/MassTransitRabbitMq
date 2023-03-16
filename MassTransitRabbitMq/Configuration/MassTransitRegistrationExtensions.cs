using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransitRabbitMq.Configuration.AppSettingsConfig;
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
                    endpointConfigurator.UseEntityFrameworkOutbox<AppDbContext>(rabbitContext);
            
                    endpointConfigurator.UseConsumeFilter(typeof(ExceptionAuditFilter<>), rabbitContext);
            
                    endpointConfigurator.ConfigureConsumer<AddPersonIntegrationCommandHandler>(rabbitContext,
                        consumerConfigurator =>
                        {
                            consumerConfigurator.UseFilter(new ExceptionAuditConsumerFilter<AddPersonIntegrationCommandHandler>());
                            
                            // --- IMPORTANT PLACE ---------------------------------------------------------------------
                            // Message retry is set on receive endpoint level
                            // Everything works as expected; outbox transaction is rolled back in EntityFrameworkOutboxContextFactory, each time retry is raised
                            endpointConfigurator.UseMessageRetry(r =>
                            {
                                r.Handle<Exception>();
                                r.Interval(2, TimeSpan.FromSeconds(10));
                            });
                        });
                });
            });
        });
    }
}