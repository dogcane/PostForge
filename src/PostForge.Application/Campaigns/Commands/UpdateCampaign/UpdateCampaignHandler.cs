using ECO.Data;
using Mediator;
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

        var result = campaign.UpdateDetails(
            request.Name,
            request.Goal,
            request.Channel,
            request.StartDateUtc,
            request.EndDateUtc);

        if (!result.Success)
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => $"{e.Context}: {e.Description}")));

        campaignRepository.Update(campaign);
        await dataContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
