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
            busConfigurator.AddDelayedMessageScheduler();
            
            busConfigurator.AddEntityFrameworkOutbox<AppDbContext>(outboxConfigurator =>
            {
                outboxConfigurator.UseBusOutbox();
                outboxConfigurator.LockStatementProvider = new PostgresLockStatementProvider();
            });
            
            busConfigurator.AddConsumer<AddPersonIntegrationCommandHandler>();

            busConfigurator.UsingRabbitMq((rabbitContext, rabbitConfigurator) =>
            {
                rabbitConfigurator.UseDelayedMessageScheduler();
                
                rabbitConfigurator.Host(rabbitMqConfig.Host, (ushort) rabbitMqConfig.Port, rabbitMqConfig.VirtualHost, 
                    hostConfigurator =>
                {
                    hostConfigurator.Username(rabbitMqConfig.Username);
                    hostConfigurator.Password(rabbitMqConfig.Password);
                });

                #region Configure bus-level filters

                rabbitConfigurator.UseConsumeFilter(typeof(HandlerTypeContextInitializerFilter<>), rabbitContext);
                
                #endregion
                
                rabbitConfigurator.ReceiveEndpoint(Endpoints.AppWorker, endpointConfigurator =>
                {
                    endpointConfigurator.UseEntityFrameworkOutbox<AppDbContext>(rabbitContext);
                    
                    endpointConfigurator.UseConsumeFilter(typeof(ExceptionAuditFilter<>), rabbitContext);
                    
                    endpointConfigurator.ConfigureDelayedRedeliveryOnEndpointLevel();
                    
                    endpointConfigurator.ConfigureConsumer<AddPersonIntegrationCommandHandler>(rabbitContext,
                        consumerConfigurator =>
                        {
                            // consumerConfigurator.ConfigureDelayedRedeliveryOnConsumerLevel(); // Looks like it doesn't work

                            consumerConfigurator.UseFilter(new HandlerTypeForwarderFilter<AddPersonIntegrationCommandHandler>());
                        }
                    );
                });
            });
        });
    }

    private static void ConfigureDelayedRedeliveryOnEndpointLevel(
        this IRabbitMqReceiveEndpointConfigurator endpointConfigurator)
    {
        endpointConfigurator.UseDelayedRedelivery(redeliveryConfigurator =>
        {
            redeliveryConfigurator.Handle<InvalidOperationException>();
            redeliveryConfigurator.Interval(1, TimeSpan.FromSeconds(5));
        });
                            
        endpointConfigurator.UseDelayedRedelivery(redeliveryConfigurator =>
        {
            redeliveryConfigurator.Handle<NullReferenceException>();
            redeliveryConfigurator.Interval(2, TimeSpan.FromSeconds(10));
        });
    }

    private static void ConfigureDelayedRedeliveryOnConsumerLevel<T>(this IConsumerConfigurator<T> consumerConfigurator)
        where T : class
    {
        consumerConfigurator.UseDelayedRedelivery(redeliveryConfigurator =>
        {
            redeliveryConfigurator.Handle<InvalidOperationException>();
            redeliveryConfigurator.Interval(1, TimeSpan.FromSeconds(5));
        });
                            
        consumerConfigurator.UseDelayedRedelivery(redeliveryConfigurator =>
        {
            redeliveryConfigurator.Handle<NullReferenceException>();
            redeliveryConfigurator.Interval(2, TimeSpan.FromSeconds(10));
        });
    }
}