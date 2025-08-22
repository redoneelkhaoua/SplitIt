using MediatR;
using TailoringApp.Application.Abstractions.Persistence;

namespace TailoringApp.Application.WorkOrders.Commands.Discount;

public sealed class ClearWorkOrderDiscountCommandHandler : IRequestHandler<ClearWorkOrderDiscountCommand, bool>
{
    private readonly IWorkOrderRepository _repo;
    public ClearWorkOrderDiscountCommandHandler(IWorkOrderRepository repo) => _repo = repo;

    public async Task<bool> Handle(ClearWorkOrderDiscountCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdForUpdateAsync(request.WorkOrderId, ct);
        if (entity is null) return false;
        try
        {
            entity.ClearDiscount();
        }
        catch (Exception)
        {
            return false;
        }
        await _repo.SaveChangesAsync(ct);
        return true;
    }
}
