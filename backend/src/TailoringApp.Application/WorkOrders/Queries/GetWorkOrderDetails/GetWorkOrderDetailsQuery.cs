using MediatR;
using TailoringApp.Application.WorkOrders.Dtos;

namespace TailoringApp.Application.WorkOrders.Queries.GetWorkOrderDetails;

public sealed record GetWorkOrderDetailsQuery(Guid CustomerId, Guid WorkOrderId) : IRequest<WorkOrderDto?>;
