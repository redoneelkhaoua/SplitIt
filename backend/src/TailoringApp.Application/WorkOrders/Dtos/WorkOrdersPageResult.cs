using TailoringApp.Application.Common.Paging;

namespace TailoringApp.Application.WorkOrders.Dtos;

public sealed record WorkOrdersPageResult : PagingResult<WorkOrderSummaryDto>
{
    public WorkOrdersPageResult(IReadOnlyList<WorkOrderSummaryDto> items, int total, int page, int pageSize)
        : base(items, total, page, pageSize) { }
}

public sealed record WorkOrderSummaryDto(
    Guid Id,
    Guid CustomerId,
    Guid? AppointmentId,
    string Currency,
    string Status,
    DateTime CreatedDate,
    decimal Subtotal,
    decimal Discount,
    decimal Total
);
