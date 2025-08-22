using MediatR;

namespace TailoringApp.Application.WorkOrders.Commands.CreateWorkOrder;

public sealed record CreateWorkOrderCommand(
    Guid CustomerId,
    string Currency,
    Guid? AppointmentId
) : IRequest<Guid>;
