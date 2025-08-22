using MediatR;

namespace TailoringApp.Application.WorkOrders.Commands.UpdateItemQuantity;

public sealed record UpdateWorkOrderItemQuantityCommand(
    Guid WorkOrderId,
    string Description,
    int Quantity
) : IRequest<bool>;
