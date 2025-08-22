using MediatR;

namespace TailoringApp.Application.Customers.Commands.RestoreCustomer;

public record RestoreCustomerCommand(Guid Id) : IRequest<bool>;
