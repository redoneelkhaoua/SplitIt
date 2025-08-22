using MediatR;

namespace TailoringApp.Application.WorkOrders.Commands.Cancel;

public sealed record CancelWorkOrderCommand(Guid WorkOrderId) : IRequest<bool>;
