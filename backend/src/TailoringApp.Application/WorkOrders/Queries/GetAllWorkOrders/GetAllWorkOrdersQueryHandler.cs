using MediatR;
using TailoringApp.Application.Abstractions.Persistence;
using TailoringApp.Application.WorkOrders.Dtos;

namespace TailoringApp.Application.WorkOrders.Queries.GetAllWorkOrders;

public sealed class GetAllWorkOrdersQueryHandler : IRequestHandler<GetAllWorkOrdersQuery, WorkOrdersPageResult>
{
    private readonly IWorkOrderRepository _repo;

    public GetAllWorkOrdersQueryHandler(IWorkOrderRepository repo) => _repo = repo;

    public async Task<WorkOrdersPageResult> Handle(GetAllWorkOrdersQuery request, CancellationToken ct)
    {
        var (items, total) = await _repo.GetAllAsync(
            request.Page,
            request.PageSize,
            request.SortBy,
            request.Desc,
            request.Status,
            request.CustomerId,
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
