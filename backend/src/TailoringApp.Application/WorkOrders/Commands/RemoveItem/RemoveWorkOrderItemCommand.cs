using MediatR;

namespace TailoringApp.Application.WorkOrders.Commands.RemoveItem;

public sealed record RemoveWorkOrderItemCommand(
    Guid WorkOrderId,
    string Description
) : IRequest<bool>;
