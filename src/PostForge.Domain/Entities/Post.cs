using ECO;
using PostForge.Domain.ValueObjects;
using Resulz;
using Resulz.Validation;

namespace PostForge.Domain.Entities;

public class Post : AggregateRoot<Guid>
{
    private readonly List<MediaAsset> _mediaAssetsField = [];
    private readonly List<SocialPlatform> _targetPlatformsField = [];

    public Guid Id => Identity;
    public string Text { get; private set; }
    public IReadOnlyList<MediaAsset> MediaAssets => _mediaAssetsField.AsReadOnly();
    public IReadOnlyList<SocialPlatform> TargetPlatforms => _targetPlatformsField.AsReadOnly();
    public Guid? CampaignId { get; private set; }
    public PostStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Post() : base(Guid.NewGuid())
    {
        Text = null!;
    }

    private Post(string text, Guid? campaignId) : base(Guid.NewGuid())
    {
        Text = text;
        CampaignId = campaignId;
        Status = PostStatus.Draft;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static OperationResult<Post> Create(string text, Guid? campaignId = null)
    {
        var result = OperationResult.MakeSuccess();
        result.With(text, "Text").Required().StringLength(5000);
        if (!result.Success)
            return result;
        return OperationResult<Post>.MakeSuccess(new Post(text, campaignId));
    }

    public OperationResult UpdateText(string text)
    {
        var result = OperationResult.MakeSuccess();
        result.With(text, "Text").Required().StringLength(5000);
        if (!result.Success)
            return result;
        Text = text;
        UpdatedAtUtc = DateTime.UtcNow;
        return OperationResult.MakeSuccess();
    }

    public OperationResult AddMedia(MediaAsset media)
    {
        var result = OperationResult.MakeSuccess();
        result.With(media, "Media").Required();
        if (!result.Success)
            return result;
        _mediaAssetsField.Add(media);
        UpdatedAtUtc = DateTime.UtcNow;
        return OperationResult.MakeSuccess();
    }

    public OperationResult RemoveMedia(MediaAsset media)
    {
        var result = OperationResult.MakeSuccess();
        result.With(media, "Media").Required();
        if (!result.Success)
            return result;
        if (!_mediaAssetsField.Remove(media))
            return OperationResult.MakeFailure(ErrorMessage.Create("Media", "Media not found in the post."));
        UpdatedAtUtc = DateTime.UtcNow;
        return OperationResult.MakeSuccess();
    }

    public OperationResult SetStatus(PostStatus newStatus)
    {
        var result = OperationResult.MakeSuccess();
        result.With(newStatus, "Status").Condition(v => Enum.IsDefined(typeof(PostStatus), v));
        if (!result.Success)
            return result;
        var oldStatus = Status;
        Status = newStatus;
        UpdatedAtUtc = DateTime.UtcNow;
        return OperationResult.MakeSuccess();
    }

    public OperationResult ScheduleForPlatform(SocialPlatform platform)
    {
        var result = OperationResult.MakeSuccess();
        result.With(platform, "Platform").Condition(v => Enum.IsDefined(typeof(SocialPlatform), v));
        if (!result.Success)
            return result;
        if (!_targetPlatformsField.Contains(platform))
        {
            _targetPlatformsField.Add(platform);
            UpdatedAtUtc = DateTime.UtcNow;
        }
        return OperationResult.MakeSuccess();
    }
}
