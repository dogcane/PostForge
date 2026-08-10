namespace PostForge.Infrastructure.Dtos;

public enum PrivacyLevel
{
    Public,
    Friends,
    Unlisted,
    Private
}

public enum CommentControlsMode
{
    Allowed,
    Moderated,
    Restricted,
    Disabled
}

public record PublishSettingsDto(
    string? Title = null,
    IReadOnlyList<string>? AltTexts = null,
    IReadOnlyList<string>? Hashtags = null,
    IReadOnlyList<string>? MentionedUsernames = null,
    IReadOnlyList<string>? UserTagUsernames = null,
    string? LocationId = null,
    string? CoverUrl = null,
    string? AudioName = null,
    IReadOnlyList<string>? CollaboratorUsernames = null,
    bool IsPaidPartnership = false,
    bool IsAiGenerated = false,
    bool ShareToFeed = true,
    PrivacyLevel? Privacy = null,
    CommentControlsMode? CommentControls = null,
    bool AllowDuet = true,
    bool AllowStitch = true);
