using MediatR;

namespace TailoringApp.Domain.Customers.Events;

public sealed record CustomerRegistered(Guid CustomerId, string Email) : INotification;
