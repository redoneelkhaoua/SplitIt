using MediatR;

namespace TailoringApp.Application.Appointments.Commands.UpdateAppointmentNotes;

public record UpdateAppointmentNotesCommand(Guid AppointmentId, Guid CustomerId, string? Notes) : IRequest<bool>;
