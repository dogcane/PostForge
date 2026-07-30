using FluentValidation;

namespace PostForge.Application.Campaigns.Commands.CreateCampaign;

public class CreateCampaignValidator : AbstractValidator<CreateCampaignCommand>
{
    public CreateCampaignValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(v => v.Goal)
            .IsInEnum();

        RuleFor(v => v.Channel)
            .IsInEnum();

        RuleFor(v => v.StartDateUtc)
            .NotEmpty();
    }
}
