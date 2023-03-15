using MassTransitRabbitMq.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.RegisterPostgresql();
builder.RegisterMassTransit();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.Run();