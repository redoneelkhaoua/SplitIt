using MediatR;
using TailoringApp.Application.Abstractions.Persistence;

namespace TailoringApp.Application.WorkOrders.Commands.Cancel;

public sealed class CancelWorkOrderCommandHandler : IRequestHandler<CancelWorkOrderCommand, bool>
{
    private readonly IWorkOrderRepository _repo;

    public CancelWorkOrderCommandHandler(IWorkOrderRepository repo) => _repo = repo;

    public async Task<bool> Handle(CancelWorkOrderCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdForUpdateAsync(request.WorkOrderId, ct);
        if (entity is null) return false;
        try
        {
            entity.Cancel();
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        await _repo.SaveChangesAsync(ct);
        return true;
    }
}
