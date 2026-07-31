using PostForge.Application.Common.Mappings;
using PostForge.Domain.Entities;
using PostForge.Domain.ValueObjects;

namespace PostForge.Application.Campaigns.DTOs;

public class CampaignDto : IMapFrom<Campaign>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public CampaignGoal Goal { get; set; }
    public CampaignChannel Channel { get; set; }
    public DateTime StartDateUtc { get; set; }
    public DateTime? EndDateUtc { get; set; }
    public List<Guid> PostIds { get; set; } = [];
    public DateTime CreatedAtUtc { get; set; }
}
