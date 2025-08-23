using MediatR;
using TailoringApp.Application.Abstractions.Persistence;
using TailoringApp.Application.WorkOrders.Dtos;

namespace TailoringApp.Application.WorkOrders.Queries.GetWorkOrderSummary;

public sealed class GetWorkOrderSummaryQueryHandler : IRequestHandler<GetWorkOrderSummaryQuery, WorkOrderSummaryDto?>
{
    private readonly IWorkOrderRepository _repo;

    public GetWorkOrderSummaryQueryHandler(IWorkOrderRepository repo) => _repo = repo;

    public async Task<WorkOrderSummaryDto?> Handle(GetWorkOrderSummaryQuery request, CancellationToken ct)
    {
        var workOrder = await _repo.GetByIdAsync(request.WorkOrderId, ct);
        if (workOrder == null) return null;

        return new WorkOrderSummaryDto(
            workOrder.Id,
            workOrder.CustomerId,
            workOrder.AppointmentId,
            workOrder.Currency,
            workOrder.Status.ToString(),
            workOrder.CreatedDate,
            workOrder.Subtotal.Amount,
            workOrder.Discount?.Amount ?? 0m,
            workOrder.Total.Amount
        );
    }
}
