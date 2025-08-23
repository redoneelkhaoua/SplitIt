using MediatR;
using TailoringApp.Application.WorkOrders.Dtos;

namespace TailoringApp.Application.WorkOrders.Queries.GetWorkOrderSummary;

public sealed record GetWorkOrderSummaryQuery(Guid WorkOrderId) : IRequest<WorkOrderSummaryDto?>;
