using Mediator;

namespace PostForge.Application.Campaigns.Commands.DeleteCampaign;

public record DeleteCampaignCommand(Guid Id) : IRequest<Unit>;
