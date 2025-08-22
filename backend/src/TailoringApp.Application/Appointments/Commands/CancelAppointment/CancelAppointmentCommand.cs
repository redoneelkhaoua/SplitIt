using MediatR;

namespace TailoringApp.Application.Appointments.Commands.CancelAppointment;

public record CancelAppointmentCommand(Guid AppointmentId, Guid CustomerId) : IRequest<bool>;
