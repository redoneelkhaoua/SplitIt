using MediatR;

namespace TailoringApp.Application.WorkOrders.Commands.Discount;

public sealed record ClearWorkOrderDiscountCommand(Guid WorkOrderId) : IRequest<bool>;
