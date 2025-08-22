using MediatR;
using TailoringApp.Application.Abstractions.Persistence;
using TailoringApp.Domain.Common;

namespace TailoringApp.Application.WorkOrders.Commands.AddItem;

public sealed class AddWorkOrderItemCommandHandler : IRequestHandler<AddWorkOrderItemCommand, bool>
{
    private readonly IWorkOrderRepository _repo;

    public AddWorkOrderItemCommandHandler(IWorkOrderRepository repo) => _repo = repo;

    public async Task<bool> Handle(AddWorkOrderItemCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdForUpdateAsync(request.WorkOrderId, ct);
        if (entity is null) return false;
        try
        {
            entity.AddItem(request.Description, request.Quantity, new Money(request.UnitPrice, request.Currency));
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        await _repo.SaveChangesAsync(ct);
        return true;
    }
}
