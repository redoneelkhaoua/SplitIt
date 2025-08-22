using FluentValidation;
using TailoringApp.Application.WorkOrders.Commands.RemoveItem;

namespace TailoringApp.Application.WorkOrders.Validators;

public sealed class RemoveWorkOrderItemCommandValidator : AbstractValidator<RemoveWorkOrderItemCommand>
{
    public RemoveWorkOrderItemCommandValidator()
    {
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(256);
    }
}
