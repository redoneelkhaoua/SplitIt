using FluentValidation;

namespace TailoringApp.Application.Appointments.Commands.ScheduleAppointment;

public class ScheduleAppointmentCommandValidator : AbstractValidator<ScheduleAppointmentCommand>
{
    public ScheduleAppointmentCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.StartUtc).LessThan(x => x.EndUtc);
        RuleFor(x => x.Notes).MaximumLength(512);
    }
}
