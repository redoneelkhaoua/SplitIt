using MediatR;
using TailoringApp.Domain.Common;

namespace TailoringApp.Application.WorkOrders.Commands.AddItem;

public sealed record AddWorkOrderItemCommand(
    Guid WorkOrderId,
    string Description,
    int Quantity,
    decimal UnitPrice,
    string Currency,
    string? GarmentType = null,
    decimal? ChestMeasurement = null,
    decimal? WaistMeasurement = null,
    decimal? HipsMeasurement = null,
    decimal? SleeveMeasurement = null,
    string? MeasurementNotes = null
) : IRequest<bool>;
