using MassTransitRabbitMq.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MassTransitRabbitMq.Configuration;

public static class PostgresqlRegistrationExtensions
{
    public static void RegisterPostgresql(this WebApplicationBuilder builder)
    {
        var postgresConnStr = builder.Configuration.GetValue<string>("Database:ConnectionString");
        builder.Services.AddDbContext<AppDbContext>(optionsBuilder =>
        {
            optionsBuilder.UseNpgsql(postgresConnStr);
        });
    }
}