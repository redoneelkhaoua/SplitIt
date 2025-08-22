using MediatR;
using TailoringApp.Application.WorkOrders.Dtos;

namespace TailoringApp.Application.WorkOrders.Queries.GetCustomerWorkOrders;

public sealed record GetCustomerWorkOrdersQuery(
    Guid CustomerId,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    bool Desc = false,
    string? Status = null,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null,
    string? Search = null
) : IRequest<(IReadOnlyList<WorkOrderDto> Items, int TotalCount)>;
