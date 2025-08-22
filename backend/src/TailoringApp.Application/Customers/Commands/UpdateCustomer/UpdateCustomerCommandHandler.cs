using MediatR;
using TailoringApp.Application.Abstractions.Persistence;
using TailoringApp.Domain.Customers.ValueObjects;

namespace TailoringApp.Application.Customers.Commands.UpdateCustomer;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, bool>
{
    private readonly ICustomerRepository _repo;
    public UpdateCustomerCommandHandler(ICustomerRepository repo) => _repo = repo;

    public async Task<bool> Handle(UpdateCustomerCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdForUpdateAsync(request.Id, ct);
        if (entity is null) return false; // not found

        entity.UpdatePersonalInfo(new PersonalInfo(request.FirstName, request.LastName, request.DateOfBirth));
        entity.UpdateContactInfo(new ContactInfo(request.Email, request.Phone ?? string.Empty, request.Address ?? string.Empty));
        entity.UpdatePreferences(new CustomerPreferences(request.StylePreference ?? string.Empty, request.FitPreference ?? string.Empty, request.FabricPreference));

    await _repo.SaveChangesAsync(ct);
    return true;
    }
}
