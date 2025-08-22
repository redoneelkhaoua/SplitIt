using FluentValidation;

namespace TailoringApp.Application.Customers.Commands.AddNote;

public class AddNoteCommandValidator : AbstractValidator<AddNoteCommand>
{
    public AddNoteCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Text).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Author).MaximumLength(100);
    }
}
