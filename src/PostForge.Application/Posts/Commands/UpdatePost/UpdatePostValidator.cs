using FluentValidation;

namespace PostForge.Application.Posts.Commands.UpdatePost;

public class UpdatePostValidator : AbstractValidator<UpdatePostCommand>
{
    public UpdatePostValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();

        RuleFor(v => v.Text)
            .NotEmpty();

        RuleForEach(v => v.Tags)
            .ChildRules(tags =>
            {
                tags.RuleFor(t => t.Platform).NotEmpty().MaximumLength(50);
                tags.RuleFor(t => t.TagType).IsInEnum();
                tags.RuleFor(t => t.Username).NotEmpty().MaximumLength(200);
            });

        RuleFor(v => v.Tags)
            .Must((command, tags) => tags is null
                || tags.All(t => command.TargetPlatforms?.Contains(t.Platform) == true))
            .WithMessage("Each tag platform must be one of the target platforms.");
    }
}
