using MediatR;
using TailoringApp.Application.Abstractions.Persistence;
using TailoringApp.Application.WorkOrders.Dtos;

namespace TailoringApp.Application.WorkOrders.Queries.GetCustomerWorkOrders;

public sealed class GetCustomerWorkOrdersQueryHandler : IRequestHandler<GetCustomerWorkOrdersQuery, (IReadOnlyList<WorkOrderDto> Items, int TotalCount)>
{
    private readonly IWorkOrderRepository _repo;

    public GetCustomerWorkOrdersQueryHandler(IWorkOrderRepository repo) => _repo = repo;

    public async Task<(IReadOnlyList<WorkOrderDto> Items, int TotalCount)> Handle(GetCustomerWorkOrdersQuery request, CancellationToken ct)
    {
        var (items, total) = await _repo.GetForCustomerAsync(request.CustomerId, request.Page, request.PageSize, request.SortBy, request.Desc, ct);
        var dtoItems = items.Select(x => new WorkOrderDto(
            x.Id,
            x.CustomerId,
            x.AppointmentId,
            x.Currency,
            x.Status,
            x.CreatedDate,
            x.Items.Select(i => new WorkOrderItemDto(i.Description, i.Quantity, i.UnitPrice.Amount, i.UnitPrice.Currency)).ToList(),
            x.Subtotal.Amount,
            x.Subtotal.Currency,
            x.Discount?.Amount ?? 0m,
            x.Total.Amount,
            x.Total.Currency
        )).ToList();
        return (dtoItems, total);
    }
}
