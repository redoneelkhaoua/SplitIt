using MediatR;
using TailoringApp.Application.Abstractions.Persistence;
using TailoringApp.Domain.Common;
using TailoringApp.Domain.WorkOrders;

namespace TailoringApp.Application.WorkOrders.Commands.AddItem;

public sealed class AddWorkOrderItemCommandHandler : IRequestHandler<AddWorkOrderItemCommand, bool>
{
    private readonly IWorkOrderRepository _repo;

    public AddWorkOrderItemCommandHandler(IWorkOrderRepository repo) => _repo = repo;

    public async Task<bool> Handle(AddWorkOrderItemCommand request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdForUpdateAsync(request.WorkOrderId, ct);
        if (entity is null) return false;
        try
        {
            // Parse garment type if provided
            GarmentType? garmentType = null;
            if (!string.IsNullOrEmpty(request.GarmentType) && 
                Enum.TryParse<GarmentType>(request.GarmentType, true, out var parsed))
            {
                garmentType = parsed;
            }

            // Create measurements if any are provided
            GarmentMeasurements? measurements = null;
            if (request.ChestMeasurement.HasValue || request.WaistMeasurement.HasValue || 
                request.HipsMeasurement.HasValue || request.SleeveMeasurement.HasValue || 
                !string.IsNullOrEmpty(request.MeasurementNotes))
            {
                measurements = new GarmentMeasurements(
                    request.ChestMeasurement ?? 0m,
                    request.WaistMeasurement ?? 0m,
                    request.HipsMeasurement ?? 0m,
                    request.SleeveMeasurement ?? 0m,
                    request.MeasurementNotes
                );
            }

            entity.AddItem(
                request.Description, 
                request.Quantity, 
                new Money(request.UnitPrice, request.Currency),
                garmentType ?? GarmentType.Other,
                measurements
            );
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        await _repo.SaveChangesAsync(ct);
        return true;
    }
}
