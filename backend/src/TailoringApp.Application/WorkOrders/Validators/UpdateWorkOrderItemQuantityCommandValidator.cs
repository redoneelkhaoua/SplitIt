using FluentValidation;
using TailoringApp.Application.WorkOrders.Commands.UpdateItemQuantity;

namespace TailoringApp.Application.WorkOrders.Validators;

public sealed class UpdateWorkOrderItemQuantityCommandValidator : AbstractValidator<UpdateWorkOrderItemQuantityCommand>
{
    public UpdateWorkOrderItemQuantityCommandValidator()
    {
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
