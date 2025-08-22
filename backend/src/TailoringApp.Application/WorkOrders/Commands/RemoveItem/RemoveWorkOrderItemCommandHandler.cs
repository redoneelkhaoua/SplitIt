using MediatR;
using TailoringApp.Application.Abstractions.Persistence;

namespace TailoringApp.Application.WorkOrders.Commands.RemoveItem;

public sealed class RemoveWorkOrderItemCommandHandler : IRequestHandler<RemoveWorkOrderItemCommand, bool>
{
    private readonly IWorkOrderRepository _repo;

    public RemoveWorkOrderItemCommandHandler(IWorkOrderRepository repo) => _repo = repo;

    public async Task<bool> Handle(RemoveWorkOrderItemCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdForUpdateAsync(request.WorkOrderId, ct);
        if (entity is null) return false;
        var removed = entity.RemoveItem(request.Description);
        if (!removed) return false;
        await _repo.SaveChangesAsync(ct);
        return true;
    }
}
