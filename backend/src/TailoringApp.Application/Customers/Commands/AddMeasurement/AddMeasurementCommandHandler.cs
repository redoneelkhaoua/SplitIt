using MediatR;
using TailoringApp.Application.Abstractions.Persistence;
using TailoringApp.Domain.Customers.Entities;

namespace TailoringApp.Application.Customers.Commands.AddMeasurement;

public class AddMeasurementCommandHandler : IRequestHandler<AddMeasurementCommand>
{
    private readonly ICustomerRepository _repo;
    public AddMeasurementCommandHandler(ICustomerRepository repo) => _repo = repo;

    public async Task Handle(AddMeasurementCommand request, CancellationToken ct)
    {
        var customer = await _repo.GetByIdForUpdateAsync(request.CustomerId, ct);
        if (customer is null) return; // or throw NotFoundException
        customer.AddMeasurement(new MeasurementRecord(request.Date, request.Chest, request.Waist, request.Hips, request.Sleeve));
        await _repo.SaveChangesAsync(ct);
    }
}
