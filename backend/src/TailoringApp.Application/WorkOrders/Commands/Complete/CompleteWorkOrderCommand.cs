using MediatR;

namespace TailoringApp.Application.WorkOrders.Commands.Complete;

public sealed record CompleteWorkOrderCommand(Guid WorkOrderId) : IRequest<bool>;
