using MediatR;

namespace TailoringApp.Application.Customers.Commands.AddMeasurement;

public record AddMeasurementCommand(Guid CustomerId, DateTime Date, decimal Chest, decimal Waist, decimal Hips, decimal Sleeve) : IRequest;
