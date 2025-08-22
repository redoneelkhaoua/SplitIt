using MediatR;

namespace TailoringApp.Application.WorkOrders.Commands.Start;

public sealed record StartWorkOrderCommand(Guid WorkOrderId) : IRequest<bool>;
