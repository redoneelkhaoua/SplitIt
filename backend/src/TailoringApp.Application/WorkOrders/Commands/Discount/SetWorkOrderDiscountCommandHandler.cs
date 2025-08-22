using MediatR;
using TailoringApp.Application.Abstractions.Persistence;
using TailoringApp.Domain.Common;

namespace TailoringApp.Application.WorkOrders.Commands.Discount;

public sealed class SetWorkOrderDiscountCommandHandler : IRequestHandler<SetWorkOrderDiscountCommand, bool>
{
    private readonly IWorkOrderRepository _repo;
    public SetWorkOrderDiscountCommandHandler(IWorkOrderRepository repo) => _repo = repo;

    public async Task<bool> Handle(SetWorkOrderDiscountCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdForUpdateAsync(request.WorkOrderId, ct);
        if (entity is null) return false;
        try
        {
            entity.SetDiscount(new Money(request.Amount, request.Currency));
        }
        catch (Exception)
        {
            return false;
        }
        await _repo.SaveChangesAsync(ct);
        return true;
    }
}
