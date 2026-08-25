using ECO;
using PostForge.Domain.ValueObjects;
using Resulz;
using Resulz.Validation;

namespace PostForge.Domain.Entities;

public class Post : AggregateRoot<Guid>
{
    #region Fields
    private readonly List<MediaAsset> _mediaAssetsField = [];
    private readonly List<string> _targetPlatformsField = [];
    private readonly List<PostTag> _tags = [];
    #endregion

    #region Properties
    public Guid Id => Identity;
    public Guid TenantId { get; private set; }
    public string Text { get; private set; }
    public IReadOnlyList<MediaAsset> MediaAssets => _mediaAssetsField.AsReadOnly();
    public IReadOnlyList<string> TargetPlatforms => _targetPlatformsField.AsReadOnly();
    public IReadOnlyList<PostTag> Tags => _tags.AsReadOnly();
    public Guid? CampaignId { get; private set; }
    public PostStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    #endregion

    #region ctor
    private Post() : base(Guid.NewGuid())
    {
        Text = null!;
    }

    private Post(string text, Guid tenantId, Guid? campaignId) : base(Guid.NewGuid())
    {
        Text = text;
        TenantId = tenantId;
        CampaignId = campaignId;
        Status = PostStatus.Draft;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }
    #endregion

    #region Methods
    protected static OperationResult Validate(string text, Guid tenantId)
    {
        var result = OperationResult.MakeSuccess();
        result
            .With(text, "Text").Required().StringLength(5000)
            .With(tenantId, "TenantId").Condition(v => v != Guid.Empty);
        return result;
    }

    public static OperationResult<Post> Create(string text, Guid tenantId, Guid? campaignId = null)
        => Validate(text, tenantId)
            .IfSuccessThenReturn<Post>(() => new Post(text, tenantId, campaignId));

    public OperationResult UpdateText(string text)
        => OperationResult.MakeSuccess()
            .With(text, "Text").Required().StringLength(5000)
            .Result
            .IfSuccess(_ =>
            {
                Text = text;
                UpdatedAtUtc = DateTime.UtcNow;
            });

    public OperationResult AddMedia(MediaAsset media)
        => OperationResult.MakeSuccess()
            .With(media, "Media").Required()
            .Result
            .IfSuccess(_ =>
            {
                _mediaAssetsField.Add(media);
                UpdatedAtUtc = DateTime.UtcNow;
            });

    public OperationResult RemoveMedia(MediaAsset media)
    {
        var validation = OperationResult.MakeSuccess()
            .With(media, "Media").Required()
            .Result;
        if (!validation.Success)
            return validation;
        if (!_mediaAssetsField.Contains(media))
            return OperationResult.MakeFailure(ErrorMessage.Create("Media", "Media not found in the post."));
        return OperationResult.MakeSuccess().IfSuccess(_ =>
        {
            _mediaAssetsField.Remove(media);
            UpdatedAtUtc = DateTime.UtcNow;
        });
    }

    public OperationResult SetStatus(PostStatus newStatus)
        => OperationResult.MakeSuccess()
            .With(newStatus, "Status").Condition(v => Enum.IsDefined(v))
            .Result
            .IfSuccess(_ =>
            {
                Status = newStatus;
                UpdatedAtUtc = DateTime.UtcNow;
            });

    public OperationResult ScheduleForPlatform(string platformIdentifier)
        => OperationResult.MakeSuccess()
            .With(platformIdentifier, "Platform").Required().StringLength(50)
            .Result
            .IfSuccess(_ =>
            {
                if (!_targetPlatformsField.Contains(platformIdentifier))
                {
                    _targetPlatformsField.Add(platformIdentifier);
                    UpdatedAtUtc = DateTime.UtcNow;
                }
            });

    public OperationResult AddTag(PostTag tag)
    {
        var validation = OperationResult.MakeSuccess()
            .With(tag, "Tag").Required()
            .Result;
        if (!validation.Success)
            return validation;
        if (!_targetPlatformsField.Contains(tag.Platform))
            return OperationResult.MakeFailure(
                ErrorMessage.Create("Platform", $"Cannot tag a user on platform '{tag.Platform}': the post is not targeted at it."));
        if (_tags.Contains(tag))
            return OperationResult.MakeFailure(
                ErrorMessage.Create("Tag", $"Tag '{tag.Username}' of type {tag.TagType} already exists for platform '{tag.Platform}'."));
        return OperationResult.MakeSuccess().IfSuccess(_ =>
        {
            _tags.Add(tag);
            UpdatedAtUtc = DateTime.UtcNow;
        });
    }

    public OperationResult SetTags(IReadOnlyList<PostTag> tags)
    {
        var validation = OperationResult.MakeSuccess()
            .With(tags, "Tags").Required()
            .Result;
        if (!validation.Success)
            return validation;
        foreach (var tag in tags)
        {
            validation = OperationResult.MakeSuccess()
                .With(tag, "Tag").Required()
                .Result;
            if (!validation.Success)
                return validation;
            if (!_targetPlatformsField.Contains(tag.Platform))
                return OperationResult.MakeFailure(
                    ErrorMessage.Create("Platform", $"Cannot tag a user on platform '{tag.Platform}': the post is not targeted at it."));
            if (tags.Count(t => t.Equals(tag)) > 1)
                return OperationResult.MakeFailure(
                    ErrorMessage.Create("Tag", $"Duplicate tag '{tag.Username}' of type {tag.TagType} for platform '{tag.Platform}'."));
        }
        return OperationResult.MakeSuccess().IfSuccess(_ =>
        {
            _tags.Clear();
            _tags.AddRange(tags);
            UpdatedAtUtc = DateTime.UtcNow;
        });
    }

    public OperationResult RemoveTag(PostTag tag)
    {
        var validation = OperationResult.MakeSuccess()
            .With(tag, "Tag").Required()
            .Result;
        if (!validation.Success)
            return validation;
        if (!_tags.Contains(tag))
            return OperationResult.MakeFailure(ErrorMessage.Create("Tag", "Tag not found in the post."));
        return OperationResult.MakeSuccess().IfSuccess(_ =>
        {
            _tags.Remove(tag);
            UpdatedAtUtc = DateTime.UtcNow;
        });
    }
    #endregion
}
