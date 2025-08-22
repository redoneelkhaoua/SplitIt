using MediatR;
using TailoringApp.Application.Abstractions.Persistence;

namespace TailoringApp.Application.Appointments.Commands.CancelAppointment;

public class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand, bool>
{
    private readonly IAppointmentRepository _repo;
    public CancelAppointmentCommandHandler(IAppointmentRepository repo) => _repo = repo;

    public async Task<bool> Handle(CancelAppointmentCommand request, CancellationToken ct)
    {
        var appt = await _repo.GetByIdAsync(request.AppointmentId, ct);
        if (appt is null || appt.CustomerId != request.CustomerId) return false;
        try
        {
            appt.Cancel();
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        await _repo.SaveChangesAsync(ct);
        return true;
    }
}
