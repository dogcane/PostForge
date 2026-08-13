using FluentValidation;

namespace PostForge.Application.Scheduling.Commands.SchedulePost;

public class SchedulePostValidator : AbstractValidator<SchedulePostCommand>
{
    public SchedulePostValidator()
    {
        RuleFor(v => v.PostId)
            .NotEmpty();

        RuleFor(v => v.Platform)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(v => v.ScheduledAtUtc)
            .NotEmpty()
            .GreaterThan(DateTime.UtcNow);
    }
}
