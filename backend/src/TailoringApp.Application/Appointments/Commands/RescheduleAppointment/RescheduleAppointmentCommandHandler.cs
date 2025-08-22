using MediatR;
using TailoringApp.Application.Abstractions.Persistence;

namespace TailoringApp.Application.Appointments.Commands.RescheduleAppointment;

public class RescheduleAppointmentCommandHandler : IRequestHandler<RescheduleAppointmentCommand, bool>
{
    private readonly IAppointmentRepository _repo;
    private readonly ICustomerRepository _customers;
    public RescheduleAppointmentCommandHandler(IAppointmentRepository repo, ICustomerRepository customers)
    {
        _repo = repo;
        _customers = customers;
    }

    public async Task<bool> Handle(RescheduleAppointmentCommand request, CancellationToken ct)
    {
        var appt = await _repo.GetByIdAsync(request.AppointmentId, ct);
        if (appt is null || appt.CustomerId != request.CustomerId) return false;
        var conflict = await _repo.HasConflictAsync(request.CustomerId, request.StartUtc, request.EndUtc, request.AppointmentId, ct);
        if (conflict) return false;
        appt.Reschedule(request.StartUtc, request.EndUtc);
        await _repo.SaveChangesAsync(ct);
        return true;
    }
}
