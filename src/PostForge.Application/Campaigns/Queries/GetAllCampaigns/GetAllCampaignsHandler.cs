using Mediator;
using PostForge.Application.Campaigns.DTOs;
using PostForge.Application.Common.Mappings;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.Campaigns.Queries.GetAllCampaigns;

public class GetAllCampaignsHandler(
    ICampaignRepository campaignRepository) : IRequestHandler<GetAllCampaignsQuery, List<CampaignDto>>
{
    public ValueTask<List<CampaignDto>> Handle(GetAllCampaignsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Domain.Entities.Campaign> query = campaignRepository;

        if (request.Goal.HasValue)
            query = query.Where(c => c.Goal == request.Goal.Value);

        if (request.Channel.HasValue)
            query = query.Where(c => c.Channel == request.Channel.Value);

        if (request.DateFrom.HasValue)
            query = query.Where(c => c.StartDateUtc >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(c => c.StartDateUtc <= request.DateTo.Value);

        var campaigns = query.ToList();

        return ValueTask.FromResult(campaigns.Select(c => c.ToDto()).ToList());
    }
}
