using FluentValidation;

namespace TailoringApp.Application.Customers.Commands.RegisterCustomer;

public class RegisterCustomerCommandValidator : AbstractValidator<RegisterCustomerCommand>
{
    public RegisterCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerNumber).NotEmpty().MaximumLength(32);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Phone).MaximumLength(32).When(x => !string.IsNullOrWhiteSpace(x.Phone));
        RuleFor(x => x.Address).MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.Address));
        RuleFor(x => x.FitPreference).MaximumLength(64).When(x => !string.IsNullOrWhiteSpace(x.FitPreference));
        RuleFor(x => x.StylePreference).MaximumLength(64).When(x => !string.IsNullOrWhiteSpace(x.StylePreference));
        RuleFor(x => x.FabricPreference).MaximumLength(64).When(x => !string.IsNullOrWhiteSpace(x.FabricPreference));
    }
}
