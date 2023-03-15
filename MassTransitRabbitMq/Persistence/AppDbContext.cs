using MassTransit;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddInboxStateEntity();
        
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Person> Persons { get; set; }
}