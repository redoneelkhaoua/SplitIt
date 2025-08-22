using MediatR;

namespace TailoringApp.Application.Customers.Queries.GetCustomerDetails;

public record GetCustomerDetailsQuery(Guid Id) : IRequest<CustomerDetailsDto?>;

public record MeasurementDto(DateTime Date, decimal Chest, decimal Waist, decimal Hips, decimal Sleeve);
public record NoteDto(DateTime Date, string Text, string? Author);

public record CustomerDetailsDto(
    Guid Id,
    string CustomerNumber,
    string FirstName,
    string LastName,
    string Email,
    DateTime RegistrationDate,
    bool Enabled,
    IReadOnlyList<MeasurementDto> Measurements,
    IReadOnlyList<NoteDto> Notes);
