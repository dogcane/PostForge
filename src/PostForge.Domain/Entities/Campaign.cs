using ECO;
using PostForge.Domain.ValueObjects;
using Resulz;
using Resulz.Validation;

namespace PostForge.Domain.Entities;

public class Campaign : AggregateRoot<Guid>
{
    #region Fields
    private readonly List<Guid> _postIdsField = [];
    #endregion

    #region Properties
    public Guid Id => Identity;
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public CampaignGoal Goal { get; private set; }
    public CampaignChannel Channel { get; private set; }
    public DateTime StartDateUtc { get; private set; }
    public DateTime? EndDateUtc { get; private set; }
    public IReadOnlyList<Guid> PostIds => _postIdsField.AsReadOnly();
    public DateTime CreatedAtUtc { get; private set; }
    #endregion

    #region ctor
    private Campaign() : base(Guid.NewGuid()) => Name = null!;

    protected Campaign(Guid tenantId, string name, CampaignGoal goal, CampaignChannel channel, DateTime startDateUtc, DateTime? endDateUtc) : base(Guid.NewGuid())
    {
        TenantId = tenantId;
        Name = name;
        Goal = goal;
        Channel = channel;
        StartDateUtc = startDateUtc;
        EndDateUtc = endDateUtc;
        CreatedAtUtc = DateTime.UtcNow;
    }
    #endregion

    #region Methods
    protected static OperationResult Validate(Guid tenantId, string name, CampaignGoal goal, CampaignChannel channel, DateTime startDateUtc, DateTime? endDateUtc = null)
    {
        var result = OperationResult.MakeSuccess();
        result
            .With(tenantId, "TenantId").Condition(v => v != Guid.Empty)
            .With(name, "Name").Required().StringLength(200)
            .With(goal, "Goal").Condition(v => Enum.IsDefined(v))
            .With(channel, "Channel").Condition(v => Enum.IsDefined(v));
        if (result.Success && endDateUtc.HasValue)
            result.With(endDateUtc.Value, "EndDate").Condition(v => v > startDateUtc);
        return result;
    }

    public static OperationResult<Campaign> Create(Guid tenantId, string name, CampaignGoal goal, CampaignChannel channel, DateTime startDateUtc, DateTime? endDateUtc = null) 
        => Validate(tenantId, name, goal, channel, startDateUtc, endDateUtc)
            .IfSuccessThenReturn<Campaign>(() => new Campaign(tenantId, name, goal, channel, startDateUtc, endDateUtc));


    public OperationResult UpdateDetails(string name, CampaignGoal goal, CampaignChannel channel, DateTime startDateUtc, DateTime? endDateUtc)
        => Validate(this.TenantId, name, goal, channel, startDateUtc, endDateUtc)
            .IfSuccess(result => { Name = name; Goal = goal; Channel = channel; StartDateUtc = startDateUtc; EndDateUtc = endDateUtc; });

    public OperationResult AddPost(Guid postId) 
        => OperationResult.MakeSuccess()
            .With(postId, "PostId").Condition(v => v != Guid.Empty)
            .With(postId, "PostId").Condition(v => !_postIdsField.Contains(v))
            .Result
            .IfSuccess(result => _postIdsField.Add(postId));

    public OperationResult RemovePost(Guid postId)
        => OperationResult.MakeSuccess()
            .With(postId, "PostId").Condition(v => v != Guid.Empty)
            .With(postId, "PostId").Condition(v => _postIdsField.Contains(v))
            .Result
            .IfSuccess(result => _postIdsField.Remove(postId));

    #endregion
}
