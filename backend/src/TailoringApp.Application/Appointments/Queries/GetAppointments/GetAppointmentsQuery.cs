using MediatR;

namespace TailoringApp.Application.Appointments.Queries.GetAppointments;

public record GetAppointmentsQuery(Guid? CustomerId, string? Status = null, int Page = 1, int PageSize = 50) : IRequest<GetAppointmentsResult>;
public record AppointmentListItem(Guid Id, Guid CustomerId, DateTime StartUtc, DateTime EndUtc, string? Notes, string Status);
public record GetAppointmentsResult(IReadOnlyList<AppointmentListItem> Items, int Total, int Page, int PageSize);
