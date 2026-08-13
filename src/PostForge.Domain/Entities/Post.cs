using ECO;
using PostForge.Domain.ValueObjects;
using Resulz;
using Resulz.Validation;

namespace PostForge.Domain.Entities;

public class Post : AggregateRoot<Guid>
{
    private readonly List<MediaAsset> _mediaAssetsField = [];
    private readonly List<string> _targetPlatformsField = [];
    private readonly List<PostTag> _tags = [];

    public Guid Id => Identity;
    public string Text { get; private set; }
    public IReadOnlyList<MediaAsset> MediaAssets => _mediaAssetsField.AsReadOnly();
    public IReadOnlyList<string> TargetPlatforms => _targetPlatformsField.AsReadOnly();
    public IReadOnlyList<PostTag> Tags => _tags.AsReadOnly();
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

    public OperationResult ScheduleForPlatform(string platformIdentifier)
    {
        var result = OperationResult.MakeSuccess();
        result.With(platformIdentifier, "Platform").Required().StringLength(50);
        if (!result.Success)
            return result;
        if (!_targetPlatformsField.Contains(platformIdentifier))
        {
            _targetPlatformsField.Add(platformIdentifier);
            UpdatedAtUtc = DateTime.UtcNow;
        }
        return OperationResult.MakeSuccess();
    }

    public OperationResult AddTag(PostTag tag)
    {
        var result = OperationResult.MakeSuccess();
        result.With(tag, "Tag").Required();
        if (!result.Success)
            return result;
        if (!_targetPlatformsField.Contains(tag.Platform))
            return OperationResult.MakeFailure(
                ErrorMessage.Create("Platform", $"Cannot tag a user on platform '{tag.Platform}': the post is not targeted at it."));
        if (_tags.Contains(tag))
            return OperationResult.MakeFailure(
                ErrorMessage.Create("Tag", $"Tag '{tag.Username}' of type {tag.TagType} already exists for platform '{tag.Platform}'."));
        _tags.Add(tag);
        UpdatedAtUtc = DateTime.UtcNow;
        return OperationResult.MakeSuccess();
    }

    public OperationResult SetTags(IReadOnlyList<PostTag> tags)
    {
        var result = OperationResult.MakeSuccess();
        result.With(tags, "Tags").Required();
        if (!result.Success)
            return result;
        foreach (var tag in tags)
        {
            result.With(tag, "Tag").Required();
            if (!result.Success)
                return result;
            if (!_targetPlatformsField.Contains(tag.Platform))
                return OperationResult.MakeFailure(
                    ErrorMessage.Create("Platform", $"Cannot tag a user on platform '{tag.Platform}': the post is not targeted at it."));
            if (tags.Count(t => t.Equals(tag)) > 1)
                return OperationResult.MakeFailure(
                    ErrorMessage.Create("Tag", $"Duplicate tag '{tag.Username}' of type {tag.TagType} for platform '{tag.Platform}'."));
        }
        _tags.Clear();
        _tags.AddRange(tags);
        UpdatedAtUtc = DateTime.UtcNow;
        return OperationResult.MakeSuccess();
    }

    public OperationResult RemoveTag(PostTag tag)
    {
        var result = OperationResult.MakeSuccess();
        result.With(tag, "Tag").Required();
        if (!result.Success)
            return result;
        if (!_tags.Remove(tag))
            return OperationResult.MakeFailure(ErrorMessage.Create("Tag", "Tag not found in the post."));
        UpdatedAtUtc = DateTime.UtcNow;
        return OperationResult.MakeSuccess();
    }
}
