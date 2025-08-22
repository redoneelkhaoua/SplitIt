using MediatR;

namespace TailoringApp.Application.Customers.Commands.UpdateCustomer;

public record UpdateCustomerCommand(
    Guid Id,
    string FirstName,
    string LastName,
    DateTime? DateOfBirth,
    string Email,
    string? Phone,
    string? Address,
    string? FitPreference,
    string? StylePreference,
    string? FabricPreference
) : IRequest<bool>;
