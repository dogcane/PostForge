using ECO.Data;
using Mediator;
using PostForge.Application.Common.Extensions;
using PostForge.Domain.Interfaces;
using PostForge.Domain.Entities;

namespace PostForge.Application.Campaigns.Commands.CreateCampaign;

public class CreateCampaignHandler(
    ICampaignRepository campaignRepository,
    IDataContext dataContext,
    ITenantContext tenantContext) : IRequestHandler<CreateCampaignCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId
            ?? throw new InvalidOperationException("A tenant context is required to create a campaign.");

        var campaign = Campaign.Create(
            tenantId,
            request.Name,
            request.Goal,
            request.Channel,
            request.StartDateUtc,
            request.EndDateUtc).EnsureSuccess();

        campaignRepository.Add(campaign);
        await dataContext.SaveChangesAsync(cancellationToken);

        return campaign.Id;
    }
}
