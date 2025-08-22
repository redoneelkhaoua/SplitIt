using MediatR;

namespace TailoringApp.Application.Appointments.Queries.GetCustomerAppointments;

public record GetCustomerAppointmentsQuery(Guid CustomerId, DateTime? FromUtc = null, DateTime? ToUtc = null, int Page = 1, int PageSize = 50) : IRequest<GetCustomerAppointmentsResult>;

public record GetCustomerAppointmentsResult(IReadOnlyList<AppointmentDto> Items, int Total, int Page, int PageSize);

public record AppointmentDto(Guid Id, DateTime StartUtc, DateTime EndUtc, string? Notes, string Status);
