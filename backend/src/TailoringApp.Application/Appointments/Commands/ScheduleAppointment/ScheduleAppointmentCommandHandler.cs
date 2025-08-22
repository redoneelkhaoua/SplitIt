using MediatR;
using TailoringApp.Application.Abstractions.Persistence;
using TailoringApp.Domain.Appointments;

namespace TailoringApp.Application.Appointments.Commands.ScheduleAppointment;

public class ScheduleAppointmentCommandHandler : IRequestHandler<ScheduleAppointmentCommand, Guid>
{
    private readonly IAppointmentRepository _apptRepo;
    private readonly ICustomerRepository _customerRepo;

    public ScheduleAppointmentCommandHandler(IAppointmentRepository apptRepo, ICustomerRepository customerRepo)
    {
        _apptRepo = apptRepo;
        _customerRepo = customerRepo;
    }

    public async Task<Guid> Handle(ScheduleAppointmentCommand request, CancellationToken ct)
    {
        var customer = await _customerRepo.GetByIdAsync(request.CustomerId, ct);
        if (customer is null) return Guid.Empty;

        var hasConflict = await _apptRepo.HasConflictAsync(request.CustomerId, request.StartUtc, request.EndUtc, null, ct);
        if (hasConflict) return Guid.Empty;

        var appt = new Appointment(request.CustomerId, request.StartUtc, request.EndUtc, request.Notes);
        await _apptRepo.AddAsync(appt, ct);
        await _apptRepo.SaveChangesAsync(ct);
        return appt.Id;
    }
}
