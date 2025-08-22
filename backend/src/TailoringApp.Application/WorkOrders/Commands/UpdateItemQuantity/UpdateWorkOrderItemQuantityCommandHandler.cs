using MediatR;
using TailoringApp.Application.Abstractions.Persistence;

namespace TailoringApp.Application.WorkOrders.Commands.UpdateItemQuantity;

public sealed class UpdateWorkOrderItemQuantityCommandHandler : IRequestHandler<UpdateWorkOrderItemQuantityCommand, bool>
{
    private readonly IWorkOrderRepository _repo;

    public UpdateWorkOrderItemQuantityCommandHandler(IWorkOrderRepository repo) => _repo = repo;

    public async Task<bool> Handle(UpdateWorkOrderItemQuantityCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdForUpdateAsync(request.WorkOrderId, ct);
        if (entity is null) return false;
    var existing = entity.Items.FirstOrDefault(i => string.Equals(i.Description, request.Description, StringComparison.OrdinalIgnoreCase));
    if (existing is null) return false;
    // Remove first and save to avoid issues with owned collection key uniqueness
    var removed = entity.RemoveItem(request.Description);
    if (!removed) return false;
    await _repo.SaveChangesAsync(ct);
    // Add with new quantity and save
    entity.AddItem(existing.Description, request.Quantity, existing.UnitPrice);
    await _repo.SaveChangesAsync(ct);
    return true;
    }
}
