using MediatR;
using Microsoft.EntityFrameworkCore;
using TailoringApp.Domain.Common;
using TailoringApp.Domain.Customers;
using TailoringApp.Domain.Customers.Entities;
using TailoringApp.Domain.Customers.ValueObjects;
using TailoringApp.Domain.Appointments;
using TailoringApp.Domain.WorkOrders;
using TailoringApp.Domain.Users;

namespace TailoringApp.Infrastructure.Persistence;

public class TailoringDbContext : DbContext
{
    private readonly IPublisher? _publisher;

    public TailoringDbContext(DbContextOptions<TailoringDbContext> options, IPublisher? publisher = null)
        : base(options)
    {
        _publisher = publisher;
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TailoringDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_publisher is null)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        var domainEvents = ChangeTracker.Entries<Entity>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }

        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            entry.Entity.ClearDomainEvents();
        }

        return result;
    }
}
