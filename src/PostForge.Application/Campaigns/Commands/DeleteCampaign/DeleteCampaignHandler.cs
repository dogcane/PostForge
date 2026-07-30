using ECO.Data;
using Mediator;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.Campaigns.Commands.DeleteCampaign;

public class DeleteCampaignHandler(
    ICampaignRepository campaignRepository,
    IDataContext dataContext) : IRequestHandler<DeleteCampaignCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await campaignRepository.LoadAsync(request.Id)
            ?? throw new KeyNotFoundException($"Campaign with Id {request.Id} was not found.");

        campaignRepository.Remove(campaign);
        await dataContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
