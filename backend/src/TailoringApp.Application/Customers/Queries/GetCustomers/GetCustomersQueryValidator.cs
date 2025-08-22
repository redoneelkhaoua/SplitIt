using FluentValidation;

namespace TailoringApp.Application.Customers.Queries.GetCustomers;

public class GetCustomersQueryValidator : AbstractValidator<GetCustomersQuery>
{
    public GetCustomersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
        RuleFor(x => x.SortDir).Must(x => string.Equals(x, "asc", StringComparison.OrdinalIgnoreCase) || string.Equals(x, "desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("sortDir must be 'asc' or 'desc'");
        RuleFor(x => x.SortBy)
            .Must(x => x is null || new[] { "firstname", "lastname", "email", "customernumber", "created", "registrationdate" }
                .Contains(x.ToLowerInvariant()))
            .WithMessage("Invalid sortBy. Allowed: firstname, lastname, email, customernumber, created, registrationdate");

        RuleFor(x => x.Status)
            .Must(x => new[] { "enabled", "disabled", "all" }.Contains(x.ToLowerInvariant()))
            .WithMessage("Invalid status. Allowed: enabled, disabled, all");
    }
}
