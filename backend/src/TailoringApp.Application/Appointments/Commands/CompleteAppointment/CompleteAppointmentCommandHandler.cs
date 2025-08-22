using MediatR;
using TailoringApp.Application.Abstractions.Persistence;

namespace TailoringApp.Application.Appointments.Commands.CompleteAppointment;

public class CompleteAppointmentCommandHandler : IRequestHandler<CompleteAppointmentCommand, bool>
{
    private readonly IAppointmentRepository _repo;
    public CompleteAppointmentCommandHandler(IAppointmentRepository repo) => _repo = repo;

    public async Task<bool> Handle(CompleteAppointmentCommand request, CancellationToken ct)
    {
        var appt = await _repo.GetByIdAsync(request.AppointmentId, ct);
        if (appt is null || appt.CustomerId != request.CustomerId) return false;
        try
        {
            appt.Complete();
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        await _repo.SaveChangesAsync(ct);
        return true;
    }
}
