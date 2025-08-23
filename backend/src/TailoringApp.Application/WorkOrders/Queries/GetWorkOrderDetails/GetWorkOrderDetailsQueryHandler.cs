using MediatR;
using TailoringApp.Application.Abstractions.Persistence;
using TailoringApp.Application.WorkOrders.Dtos;

namespace TailoringApp.Application.WorkOrders.Queries.GetWorkOrderDetails;

public sealed class GetWorkOrderDetailsQueryHandler : IRequestHandler<GetWorkOrderDetailsQuery, WorkOrderDto?>
{
    private readonly IWorkOrderRepository _repo;

    public GetWorkOrderDetailsQueryHandler(IWorkOrderRepository repo) => _repo = repo;

    public async Task<WorkOrderDto?> Handle(GetWorkOrderDetailsQuery request, CancellationToken ct)
    {
        var wo = await _repo.GetByIdAsync(request.WorkOrderId, ct);
        if (wo is null || wo.CustomerId != request.CustomerId || !wo.Enabled) return null;
        return new WorkOrderDto(
            wo.Id,
            wo.CustomerId,
            wo.AppointmentId,
            wo.Currency,
            wo.Status.ToString(),
            wo.CreatedDate,
            wo.Items.Select(i => new WorkOrderItemDto(
                i.Description, 
                i.Quantity, 
                i.UnitPrice.Amount, 
                i.UnitPrice.Currency,
                i.GarmentType.ToString(),
                i.Measurements?.Chest,
                i.Measurements?.Waist,
                i.Measurements?.Hips,
                i.Measurements?.Sleeve,
                i.Measurements?.Notes
            )).ToList(),
            wo.Subtotal.Amount,
            wo.Subtotal.Currency,
            wo.Discount?.Amount ?? 0m,
            wo.Total.Amount,
            wo.Total.Currency
        );
    }
}
