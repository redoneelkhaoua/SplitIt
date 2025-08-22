using MediatR;
using TailoringApp.Application.Abstractions.Persistence;

namespace TailoringApp.Application.WorkOrders.Commands.Complete;

public sealed class CompleteWorkOrderCommandHandler : IRequestHandler<CompleteWorkOrderCommand, bool>
{
    private readonly IWorkOrderRepository _repo;

    public CompleteWorkOrderCommandHandler(IWorkOrderRepository repo) => _repo = repo;

    public async Task<bool> Handle(CompleteWorkOrderCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdForUpdateAsync(request.WorkOrderId, ct);
        if (entity is null) return false;
        try
        {
            entity.Complete();
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        await _repo.SaveChangesAsync(ct);
        return true;
    }
}
