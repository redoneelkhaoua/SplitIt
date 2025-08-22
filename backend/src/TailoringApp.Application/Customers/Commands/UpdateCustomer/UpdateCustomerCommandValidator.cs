using FluentValidation;

namespace TailoringApp.Application.Customers.Commands.UpdateCustomer;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).MaximumLength(32);
        RuleFor(x => x.Address).MaximumLength(256);
        RuleFor(x => x.FitPreference).MaximumLength(64);
        RuleFor(x => x.StylePreference).MaximumLength(64);
        RuleFor(x => x.FabricPreference).MaximumLength(64);
    }
}
