using MediatR;
using TailoringApp.Application.Abstractions.Persistence;

namespace TailoringApp.Application.Appointments.Queries.GetCustomerAppointments;

public class GetCustomerAppointmentsQueryHandler : IRequestHandler<GetCustomerAppointmentsQuery, GetCustomerAppointmentsResult>
{
    private readonly IAppointmentRepository _repo;
    public GetCustomerAppointmentsQueryHandler(IAppointmentRepository repo) => _repo = repo;

    public async Task<GetCustomerAppointmentsResult> Handle(GetCustomerAppointmentsQuery request, CancellationToken ct)
    {
        var (items, total) = await _repo.GetForCustomerAsync(request.CustomerId, request.FromUtc, request.ToUtc, request.Page, request.PageSize, ct);
        var dtos = items.Select(a => new AppointmentDto(a.Id, a.StartUtc, a.EndUtc, a.Notes, a.Status.ToString())).ToList();
        return new GetCustomerAppointmentsResult(dtos, total, request.Page, request.PageSize);
    }
}
