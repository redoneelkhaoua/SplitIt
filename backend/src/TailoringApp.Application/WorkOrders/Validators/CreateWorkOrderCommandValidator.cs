using FluentValidation;
using TailoringApp.Application.WorkOrders.Commands.CreateWorkOrder;

namespace TailoringApp.Application.WorkOrders.Validators;

public sealed class CreateWorkOrderCommandValidator : AbstractValidator<CreateWorkOrderCommand>
{
    public CreateWorkOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Currency).Length(3);
    }
}
