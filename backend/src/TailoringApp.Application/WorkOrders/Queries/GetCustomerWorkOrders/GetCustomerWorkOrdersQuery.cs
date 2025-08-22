using MediatR;
using TailoringApp.Application.WorkOrders.Dtos;

namespace TailoringApp.Application.WorkOrders.Queries.GetCustomerWorkOrders;

public sealed record GetCustomerWorkOrdersQuery(
    Guid CustomerId,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    bool Desc = false
) : IRequest<(IReadOnlyList<WorkOrderDto> Items, int TotalCount)>;
