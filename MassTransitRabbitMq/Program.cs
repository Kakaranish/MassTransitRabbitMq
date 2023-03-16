using MassTransitRabbitMq.Configuration;
using MassTransitRabbitMq.Configuration.MassTransit;
using MassTransitRabbitMq.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.RegisterPostgresql();
builder.RegisterMassTransit();

var app = builder.Build();
var retryConfigurationProvider = app.Services.GetRequiredService<IConsumerRetryConfigurationProvider>();
retryConfigurationProvider.AddConfiguration<AddPersonIntegrationCommandHandler>(2, TimeSpan.FromSeconds(10));


app.UseAuthorization();
app.MapControllers();

app.Run();