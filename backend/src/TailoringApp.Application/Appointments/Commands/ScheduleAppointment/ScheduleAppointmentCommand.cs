using MediatR;

namespace TailoringApp.Application.Appointments.Commands.ScheduleAppointment;

public record ScheduleAppointmentCommand(Guid CustomerId, DateTime StartUtc, DateTime EndUtc, string? Notes) : IRequest<Guid>;
