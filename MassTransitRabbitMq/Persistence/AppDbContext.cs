using MassTransitRabbitMq.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace MassTransitRabbitMq.Persistence;

public class AppDbContext : DbContext
{
    protected AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Person> Persons { get; set; }
}