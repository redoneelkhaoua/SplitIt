using FluentValidation;
using TailoringApp.Application.WorkOrders.Commands.AddItem;

namespace TailoringApp.Application.WorkOrders.Validators;

public sealed class AddWorkOrderItemCommandValidator : AbstractValidator<AddWorkOrderItemCommand>
{
    public AddWorkOrderItemCommandValidator()
    {
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).Length(3);
    }
}
