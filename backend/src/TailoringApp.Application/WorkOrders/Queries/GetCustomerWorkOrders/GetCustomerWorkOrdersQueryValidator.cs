using FluentValidation;

namespace TailoringApp.Application.WorkOrders.Queries.GetCustomerWorkOrders;

public sealed class GetCustomerWorkOrdersQueryValidator : AbstractValidator<GetCustomerWorkOrdersQuery>
{
    private static readonly string[] AllowedSort = new[] { "created", "status" };
    private static readonly string[] AllowedStatus = new[] { "pending", "inprogress", "completed", "cancelled" };

    public GetCustomerWorkOrdersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
        RuleFor(x => x.SortBy)
            .Must(x => x is null || AllowedSort.Contains(x.ToLowerInvariant()))
            .WithMessage($"Invalid sortBy. Allowed: {string.Join(", ", AllowedSort)}");
        RuleFor(x => x.Status)
            .Must(s => s is null || AllowedStatus.Contains(s.ToLowerInvariant()))
            .WithMessage($"Invalid status. Allowed: {string.Join(", ", AllowedStatus)}");
        RuleFor(x => x)
            .Must(q => !(q.FromUtc.HasValue && q.ToUtc.HasValue) || q.FromUtc <= q.ToUtc)
            .WithMessage("fromUtc must be <= toUtc");
    }
}
