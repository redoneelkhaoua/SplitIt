using MediatR;
using TailoringApp.Domain.Common;

namespace TailoringApp.Application.WorkOrders.Commands.AddItem;

public sealed record AddWorkOrderItemCommand(
    Guid WorkOrderId,
    string Description,
    int Quantity,
    decimal UnitPrice,
    string Currency
) : IRequest<bool>;
