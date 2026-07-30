using Mediator;
using PostForge.Application.Campaigns.DTOs;
using PostForge.Domain.ValueObjects;

namespace PostForge.Application.Campaigns.Queries.GetAllCampaigns;

public record GetAllCampaignsQuery(
    CampaignGoal? Goal,
    CampaignChannel? Channel,
    DateTime? DateFrom,
    DateTime? DateTo) : IRequest<List<CampaignDto>>;
