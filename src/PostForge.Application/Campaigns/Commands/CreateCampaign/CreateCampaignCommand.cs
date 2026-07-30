using Mediator;
using PostForge.Domain.ValueObjects;

namespace PostForge.Application.Campaigns.Commands.CreateCampaign;

public record CreateCampaignCommand(
    string Name,
    CampaignGoal Goal,
    CampaignChannel Channel,
    DateTime StartDateUtc,
    DateTime? EndDateUtc) : IRequest<Guid>;
