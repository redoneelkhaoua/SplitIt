using MediatR;

namespace TailoringApp.Application.Appointments.Commands.RescheduleAppointment;

public record RescheduleAppointmentCommand(Guid AppointmentId, Guid CustomerId, DateTime StartUtc, DateTime EndUtc) : IRequest<bool>;
