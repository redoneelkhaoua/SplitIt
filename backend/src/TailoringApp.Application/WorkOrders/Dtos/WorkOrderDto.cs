using TailoringApp.Domain.WorkOrders;

namespace TailoringApp.Application.WorkOrders.Dtos;

public sealed record WorkOrderItemDto(
	string Description, 
	int Quantity, 
	decimal UnitPrice, 
	string Currency, 
	string GarmentType, 
	decimal? ChestMeasurement,
	decimal? WaistMeasurement,
	decimal? HipsMeasurement,
	decimal? SleeveMeasurement,
	string? MeasurementNotes
);
public sealed record WorkOrderDto(
	Guid Id,
	Guid CustomerId,
	Guid? AppointmentId,
	string Currency,
	string Status,
	DateTime CreatedDate,
	IReadOnlyList<WorkOrderItemDto> Items,
	decimal Subtotal,
	string SubtotalCurrency,
	decimal Discount,
	decimal Total,
	string TotalCurrency
);
