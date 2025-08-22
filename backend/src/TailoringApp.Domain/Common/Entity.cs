using TailoringApp.Domain.Abstractions;
using MediatR;

namespace TailoringApp.Domain.Common;

public abstract class Entity
{
    private readonly List<INotification> _domainEvents = new();
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedDate { get; protected set; } = DateTime.UtcNow;
    public bool Enabled { get; protected set; } = true;

    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    protected void Raise(INotification evt) => _domainEvents.Add(evt);
    public void ClearDomainEvents() => _domainEvents.Clear();

    public void SoftDelete() => Enabled = false;
    public void Restore() => Enabled = true;
}
