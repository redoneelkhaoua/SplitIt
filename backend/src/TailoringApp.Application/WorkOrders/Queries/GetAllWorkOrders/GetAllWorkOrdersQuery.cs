using MediatR;
using TailoringApp.Application.WorkOrders.Dtos;

namespace TailoringApp.Application.WorkOrders.Queries.GetAllWorkOrders;

public sealed record GetAllWorkOrdersQuery(
    int Page,
    int PageSize,
    string? SortBy,
    bool Desc,
    string? Status,
    Guid? CustomerId,
    DateTime? FromUtc,
    DateTime? ToUtc,
    string? Search
) : IRequest<WorkOrdersPageResult>;
