using FluentValidation;

namespace PostForge.Application.Campaigns.Commands.UpdateCampaign;

public class UpdateCampaignValidator : AbstractValidator<UpdateCampaignCommand>
{
    public UpdateCampaignValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();

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
