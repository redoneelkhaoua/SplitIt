using MediatR;
using TailoringApp.Application.Abstractions.Persistence;
using TailoringApp.Domain.WorkOrders;

namespace TailoringApp.Application.WorkOrders.Commands.CreateWorkOrder;

public sealed class CreateWorkOrderCommandHandler : IRequestHandler<CreateWorkOrderCommand, Guid>
{
    private readonly ICustomerRepository _customers;
    private readonly IWorkOrderRepository _repo;
    private readonly IAppointmentRepository _appointments;

    public CreateWorkOrderCommandHandler(ICustomerRepository customers, IWorkOrderRepository repo, IAppointmentRepository appointments)
    {
        _customers = customers;
        _repo = repo;
        _appointments = appointments;
    }

    public async Task<Guid> Handle(CreateWorkOrderCommand request, CancellationToken ct)
    {
        var customer = await _customers.GetByIdAsync(request.CustomerId, ct);
        if (customer is null) return Guid.Empty;

        if (request.AppointmentId is Guid apptId)
        {
            var appt = await _appointments.GetByIdAsync(apptId, ct);
            if (appt is null || appt.CustomerId != request.CustomerId)
                return Guid.Empty;
        }

        var wo = WorkOrder.Create(request.CustomerId, request.Currency, request.AppointmentId);
        await _repo.AddAsync(wo, ct);
        await _repo.SaveChangesAsync(ct);
        return wo.Id;
    }
}
