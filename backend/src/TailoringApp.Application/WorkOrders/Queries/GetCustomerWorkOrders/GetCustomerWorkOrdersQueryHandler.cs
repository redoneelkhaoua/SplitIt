using MediatR;
using TailoringApp.Application.Abstractions.Persistence;
using TailoringApp.Application.WorkOrders.Dtos;
using TailoringApp.Application.Common.Paging;

namespace TailoringApp.Application.WorkOrders.Queries.GetCustomerWorkOrders;

public sealed class GetCustomerWorkOrdersQueryHandler : IRequestHandler<GetCustomerWorkOrdersQuery, WorkOrdersPageResult>
{
    private readonly IWorkOrderRepository _repo;

    public GetCustomerWorkOrdersQueryHandler(IWorkOrderRepository repo) => _repo = repo;

    public async Task<WorkOrdersPageResult> Handle(GetCustomerWorkOrdersQuery request, CancellationToken ct)
    {
        var (items, total) = await _repo.GetForCustomerAsync(
            request.CustomerId,
            request.Page,
            request.PageSize,
            request.SortBy,
            request.Desc,
            request.Status,
            request.FromUtc,
            request.ToUtc,
            request.Search,
            ct);
        var dtoItems = items.Select(x => new WorkOrderSummaryDto(
            x.Id,
            x.CustomerId,
            x.AppointmentId,
            x.Currency,
            x.Status.ToString(),
            x.CreatedDate,
            x.Subtotal.Amount,
            x.Discount?.Amount ?? 0m,
            x.Total.Amount
        )).ToList();
        return new WorkOrdersPageResult(dtoItems, total, request.Page, request.PageSize);
    }
}
