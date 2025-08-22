using FluentValidation;

namespace TailoringApp.Application.Customers.Commands.AddMeasurement;

public class AddMeasurementCommandValidator : AbstractValidator<AddMeasurementCommand>
{
    public AddMeasurementCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.Chest).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Waist).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Hips).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Sleeve).GreaterThanOrEqualTo(0);
    }
}
