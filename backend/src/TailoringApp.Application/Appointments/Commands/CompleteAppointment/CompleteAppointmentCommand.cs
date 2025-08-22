using MediatR;

namespace TailoringApp.Application.Appointments.Commands.CompleteAppointment;

public record CompleteAppointmentCommand(Guid AppointmentId, Guid CustomerId) : IRequest<bool>;
