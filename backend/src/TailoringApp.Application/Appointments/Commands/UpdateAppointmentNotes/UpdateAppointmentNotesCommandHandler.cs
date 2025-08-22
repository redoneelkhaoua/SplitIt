using MediatR;
using TailoringApp.Application.Abstractions.Persistence;

namespace TailoringApp.Application.Appointments.Commands.UpdateAppointmentNotes;

public class UpdateAppointmentNotesCommandHandler : IRequestHandler<UpdateAppointmentNotesCommand, bool>
{
    private readonly IAppointmentRepository _repo;
    public UpdateAppointmentNotesCommandHandler(IAppointmentRepository repo) => _repo = repo;

    public async Task<bool> Handle(UpdateAppointmentNotesCommand request, CancellationToken ct)
    {
        var appt = await _repo.GetByIdAsync(request.AppointmentId, ct);
        if (appt is null || appt.CustomerId != request.CustomerId) return false;
    appt.UpdateNotes(request.Notes);
        await _repo.SaveChangesAsync(ct);
        return true;
    }
}
