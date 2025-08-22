using MediatR;

namespace TailoringApp.Application.Customers.Commands.RegisterCustomer;

public record RegisterCustomerCommand(
    string CustomerNumber,
    string FirstName,
    string LastName,
    DateTime? DateOfBirth,
    string Email,
    string? Phone,
    string? Address,
    string? FitPreference,
    string? StylePreference,
    string? FabricPreference
) : IRequest<Guid>;
