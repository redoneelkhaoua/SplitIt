using MediatR;
using TailoringApp.Application.Abstractions.Persistence;

namespace TailoringApp.Application.Appointments.Queries.GetAppointments;

public class GetAppointmentsQueryHandler : IRequestHandler<GetAppointmentsQuery, GetAppointmentsResult>
{
    private readonly IAppointmentRepository _repo;
    public GetAppointmentsQueryHandler(IAppointmentRepository repo) => _repo = repo;

    public async Task<GetAppointmentsResult> Handle(GetAppointmentsQuery request, CancellationToken ct)
    {
    var (items, total) = await _repo.GetPagedAsync(request.CustomerId, request.Status, request.Page, request.PageSize, ct);
        var dtos = items.Select(a => new AppointmentListItem(a.Id, a.CustomerId, a.StartUtc, a.EndUtc, a.Notes, a.Status.ToString())).ToList();
        return new GetAppointmentsResult(dtos, total, request.Page, request.PageSize);
    }
}
