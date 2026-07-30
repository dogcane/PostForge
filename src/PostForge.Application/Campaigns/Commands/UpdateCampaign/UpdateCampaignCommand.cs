using Mediator;
using PostForge.Domain.ValueObjects;

namespace PostForge.Application.Campaigns.Commands.UpdateCampaign;

public record UpdateCampaignCommand(
    Guid Id,
    string Name,
    CampaignGoal Goal,
    CampaignChannel Channel,
    DateTime StartDateUtc,
    DateTime? EndDateUtc) : IRequest<Unit>;
