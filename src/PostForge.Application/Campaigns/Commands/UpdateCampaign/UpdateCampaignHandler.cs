using ECO.Data;
using Mediator;
using PostForge.Application.Common.Extensions;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.Campaigns.Commands.UpdateCampaign;

public class UpdateCampaignHandler(
    ICampaignRepository campaignRepository,
    IDataContext dataContext) : IRequestHandler<UpdateCampaignCommand, Unit>
{
    public async ValueTask<Unit> Handle(UpdateCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await campaignRepository.LoadAsync(request.Id)
            ?? throw new KeyNotFoundException($"Campaign with Id {request.Id} was not found.");

        campaign.UpdateDetails(
            request.Name,
            request.Goal,
            request.Channel,
            request.StartDateUtc,
            request.EndDateUtc).EnsureSuccess();

        campaignRepository.Update(campaign);
        await dataContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
