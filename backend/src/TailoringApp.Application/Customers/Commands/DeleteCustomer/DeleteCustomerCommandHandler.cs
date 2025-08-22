using MediatR;
using TailoringApp.Application.Abstractions.Persistence;

namespace TailoringApp.Application.Customers.Commands.DeleteCustomer;

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, bool>
{
    private readonly ICustomerRepository _repo;
    public DeleteCustomerCommandHandler(ICustomerRepository repo) => _repo = repo;

    public async Task<bool> Handle(DeleteCustomerCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdForUpdateAsync(request.Id, ct);
        if (entity is null) return false; // not found
        entity.SoftDelete();
        await _repo.SaveChangesAsync(ct);
        return true;
    }
}
