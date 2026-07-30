using ECO.Data;
using Mediator;
using PostForge.Domain.Interfaces;
using PostForge.Domain.Entities;

namespace PostForge.Application.Campaigns.Commands.CreateCampaign;

public class CreateCampaignHandler(
    ICampaignRepository campaignRepository,
    IDataContext dataContext) : IRequestHandler<CreateCampaignCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        var result = Campaign.Create(
            request.Name,
            request.Goal,
            request.Channel,
            request.StartDateUtc,
            request.EndDateUtc);

        if (!result.Success)
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => $"{e.Context}: {e.Description}")));

        var campaign = result.Value!;
        campaignRepository.Add(campaign);
        await dataContext.SaveChangesAsync(cancellationToken);

        return campaign.Id;
    }
}
