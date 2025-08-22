using MediatR;
using TailoringApp.Application.Abstractions.Persistence;

namespace TailoringApp.Application.Customers.Commands.RestoreCustomer;

public class RestoreCustomerCommandHandler : IRequestHandler<RestoreCustomerCommand, bool>
{
    private readonly ICustomerRepository _repo;
    public RestoreCustomerCommandHandler(ICustomerRepository repo) => _repo = repo;

    public async Task<bool> Handle(RestoreCustomerCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdIncludingDeletedAsync(request.Id, ct);
        if (entity is null) return false; // not found
        entity.Restore();
        await _repo.SaveChangesAsync(ct);
        return true;
    }
}
