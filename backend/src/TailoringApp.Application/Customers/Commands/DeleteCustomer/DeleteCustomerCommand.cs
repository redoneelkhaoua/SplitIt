using MediatR;

namespace TailoringApp.Application.Customers.Commands.DeleteCustomer;

public record DeleteCustomerCommand(Guid Id) : IRequest<bool>;
