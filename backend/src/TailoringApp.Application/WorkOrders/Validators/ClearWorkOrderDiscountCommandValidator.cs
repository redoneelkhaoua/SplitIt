using FluentValidation;
using TailoringApp.Application.WorkOrders.Commands.Discount;

namespace TailoringApp.Application.WorkOrders.Validators;

public sealed class ClearWorkOrderDiscountCommandValidator : AbstractValidator<ClearWorkOrderDiscountCommand>
{
    public ClearWorkOrderDiscountCommandValidator()
    {
        RuleFor(x => x.WorkOrderId).NotEmpty();
    }
}
