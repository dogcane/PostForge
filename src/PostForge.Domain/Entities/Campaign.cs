using ECO;
using PostForge.Domain.ValueObjects;
using Resulz;
using Resulz.Validation;

namespace PostForge.Domain.Entities;

public class Campaign : AggregateRoot<Guid>
{
    private readonly List<Guid> _postIdsField = [];

    public Guid Id => Identity;
    public string Name { get; private set; }
    public CampaignGoal Goal { get; private set; }
    public CampaignChannel Channel { get; private set; }
    public DateTime StartDateUtc { get; private set; }
    public DateTime? EndDateUtc { get; private set; }
    public IReadOnlyList<Guid> PostIds => _postIdsField.AsReadOnly();
    public DateTime CreatedAtUtc { get; private set; }

    private Campaign() : base(Guid.NewGuid())
    {
        Name = null!;
    }

    private Campaign(string name, CampaignGoal goal, CampaignChannel channel, DateTime startDateUtc, DateTime? endDateUtc) : base(Guid.NewGuid())
    {
        Name = name;
        Goal = goal;
        Channel = channel;
        StartDateUtc = startDateUtc;
        EndDateUtc = endDateUtc;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static OperationResult<Campaign> Create(string name, CampaignGoal goal, CampaignChannel channel, DateTime startDateUtc, DateTime? endDateUtc = null)
    {
        var result = OperationResult.MakeSuccess();
        result
            .With(name, "Name").Required().StringLength(200)
            .With(goal, "Goal").Condition(v => Enum.IsDefined(typeof(CampaignGoal), v))
            .With(channel, "Channel").Condition(v => Enum.IsDefined(typeof(CampaignChannel), v));
        if (result.Success && endDateUtc.HasValue)
            result.With(endDateUtc.Value, "EndDate").Condition(v => v > startDateUtc);
        if (!result.Success)
            return result;
        return OperationResult<Campaign>.MakeSuccess(new Campaign(name, goal, channel, startDateUtc, endDateUtc));
    }

    public OperationResult UpdateDetails(string name, CampaignGoal goal, CampaignChannel channel, DateTime startDateUtc, DateTime? endDateUtc)
    {
        var result = OperationResult.MakeSuccess();
        result
            .With(name, "Name").Required().StringLength(200)
            .With(goal, "Goal").Condition(v => Enum.IsDefined(typeof(CampaignGoal), v))
            .With(channel, "Channel").Condition(v => Enum.IsDefined(typeof(CampaignChannel), v));
        if (result.Success && endDateUtc.HasValue)
            result.With(endDateUtc.Value, "EndDate").Condition(v => v > startDateUtc);
        if (!result.Success)
            return result;
        Name = name;
        Goal = goal;
        Channel = channel;
        StartDateUtc = startDateUtc;
        EndDateUtc = endDateUtc;
        return OperationResult.MakeSuccess();
    }

    public OperationResult AddPost(Guid postId)
    {
        var result = OperationResult.MakeSuccess();
        result.With(postId, "PostId").Condition(v => v != Guid.Empty);
        if (!result.Success)
            return result;
        if (!_postIdsField.Contains(postId))
            _postIdsField.Add(postId);
        return OperationResult.MakeSuccess();
    }

    public OperationResult RemovePost(Guid postId)
    {
        var result = OperationResult.MakeSuccess();
        result.With(postId, "PostId").Condition(v => v != Guid.Empty);
        if (!result.Success)
            return result;
        if (!_postIdsField.Remove(postId))
            return OperationResult.MakeFailure(ErrorMessage.Create("PostId", "Post ID not found in campaign."));
        return OperationResult.MakeSuccess();
    }
}
