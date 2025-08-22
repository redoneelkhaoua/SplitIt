using FluentValidation;
using TailoringApp.Application.WorkOrders.Commands.Discount;

namespace TailoringApp.Application.WorkOrders.Validators;

public sealed class SetWorkOrderDiscountCommandValidator : AbstractValidator<SetWorkOrderDiscountCommand>
{
    public SetWorkOrderDiscountCommandValidator()
    {
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).Length(3);
    }
}
