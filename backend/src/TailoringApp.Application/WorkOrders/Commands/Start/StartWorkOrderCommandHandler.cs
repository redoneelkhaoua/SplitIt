using MediatR;
using TailoringApp.Application.Abstractions.Persistence;

namespace TailoringApp.Application.WorkOrders.Commands.Start;

public sealed class StartWorkOrderCommandHandler : IRequestHandler<StartWorkOrderCommand, bool>
{
    private readonly IWorkOrderRepository _repo;

    public StartWorkOrderCommandHandler(IWorkOrderRepository repo) => _repo = repo;

    public async Task<bool> Handle(StartWorkOrderCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdForUpdateAsync(request.WorkOrderId, ct);
        if (entity is null) return false;
        try
        {
            entity.Start();
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        await _repo.SaveChangesAsync(ct);
        return true;
    }
}
