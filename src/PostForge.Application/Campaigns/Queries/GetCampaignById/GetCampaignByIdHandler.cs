using AutoMapper;
using Mediator;
using PostForge.Application.Campaigns.DTOs;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.Campaigns.Queries.GetCampaignById;

public class GetCampaignByIdHandler(
    ICampaignRepository campaignRepository,
    IMapper mapper) : IRequestHandler<GetCampaignByIdQuery, CampaignDto?>
{
    public async ValueTask<CampaignDto?> Handle(GetCampaignByIdQuery request, CancellationToken cancellationToken)
    {
        var campaign = await campaignRepository.LoadAsync(request.Id);

        return campaign is null ? null : mapper.Map<CampaignDto>(campaign);
    }
}
