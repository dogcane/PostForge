using Mediator;
using PostForge.Application.Campaigns.DTOs;

namespace PostForge.Application.Campaigns.Queries.GetCampaignById;

public record GetCampaignByIdQuery(Guid Id) : IRequest<CampaignDto?>;
