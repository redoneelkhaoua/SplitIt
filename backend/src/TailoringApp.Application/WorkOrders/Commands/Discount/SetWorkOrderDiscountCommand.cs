using MediatR;

namespace TailoringApp.Application.WorkOrders.Commands.Discount;

public sealed record SetWorkOrderDiscountCommand(Guid WorkOrderId, decimal Amount, string Currency) : IRequest<bool>;
