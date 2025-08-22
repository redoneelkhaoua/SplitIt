using MediatR;
using TailoringApp.Application.Abstractions.Persistence;
using TailoringApp.Domain.Customers;
using TailoringApp.Domain.Customers.ValueObjects;

namespace TailoringApp.Application.Customers.Commands.RegisterCustomer;

public class RegisterCustomerCommandHandler : IRequestHandler<RegisterCustomerCommand, Guid>
{
    private readonly ICustomerRepository _repo;

    public RegisterCustomerCommandHandler(ICustomerRepository repo)
    {
        _repo = repo;
    }

    public async Task<Guid> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        var personal = new PersonalInfo(request.FirstName, request.LastName, request.DateOfBirth);
        var contact = new ContactInfo(request.Email, request.Phone ?? string.Empty, request.Address ?? string.Empty);
        var prefs = new CustomerPreferences(request.StylePreference ?? string.Empty, request.FitPreference ?? string.Empty, request.FabricPreference);

        var customer = new Customer(request.CustomerNumber, personal, contact, prefs);

        await _repo.AddAsync(customer, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        return customer.Id;
    }
}
