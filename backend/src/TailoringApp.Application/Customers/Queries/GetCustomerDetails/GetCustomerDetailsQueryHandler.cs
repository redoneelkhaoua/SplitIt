using MediatR;
using TailoringApp.Application.Abstractions.Persistence;

namespace TailoringApp.Application.Customers.Queries.GetCustomerDetails;

public class GetCustomerDetailsQueryHandler : IRequestHandler<GetCustomerDetailsQuery, CustomerDetailsDto?>
{
    private readonly ICustomerRepository _repo;
    public GetCustomerDetailsQueryHandler(ICustomerRepository repo) => _repo = repo;

    public async Task<CustomerDetailsDto?> Handle(GetCustomerDetailsQuery request, CancellationToken ct)
    {
        var c = await _repo.GetByIdWithDetailsAsync(request.Id, ct);
        if (c is null) return null;
        var measurements = c.MeasurementHistory
            .OrderByDescending(m => m.Date)
            .Select(m => new MeasurementDto(m.Date, m.Chest, m.Waist, m.Hips, m.Sleeve))
            .ToList();
        var notes = c.Notes
            .OrderByDescending(n => n.Date)
            .Select(n => new NoteDto(n.Date, n.Text, n.Author))
            .ToList();
        return new CustomerDetailsDto(
            c.Id,
            c.CustomerNumber,
            c.PersonalInfo.FirstName,
            c.PersonalInfo.LastName,
            c.ContactInfo.Email,
            c.RegistrationDate,
            c.Enabled,
            measurements,
            notes
        );
    }
}
