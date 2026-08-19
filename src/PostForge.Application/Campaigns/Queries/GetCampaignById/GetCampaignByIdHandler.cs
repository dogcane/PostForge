using Mediator;
using PostForge.Application.Campaigns.DTOs;
using PostForge.Application.Common.Mappings;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.Campaigns.Queries.GetCampaignById;

public class GetCampaignByIdHandler(
    ICampaignRepository campaignRepository) : IRequestHandler<GetCampaignByIdQuery, CampaignDto?>
{
    public async ValueTask<CampaignDto?> Handle(GetCampaignByIdQuery request, CancellationToken cancellationToken)
    {
        var campaign = await campaignRepository.LoadAsync(request.Id);

        return campaign is null ? null : campaign.ToDto();
    }
}
