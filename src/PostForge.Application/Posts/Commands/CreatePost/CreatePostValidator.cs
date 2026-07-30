using FluentValidation;

namespace PostForge.Application.Posts.Commands.CreatePost;

public class CreatePostValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostValidator()
    {
        RuleFor(v => v.Text)
            .NotEmpty()
            .MaximumLength(5000);
    }
}
