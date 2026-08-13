namespace PostForge.Domain.Providers.Contracts;

public enum CommentModerationAction
{
    Hide,
    Unhide,
    Delete,
    Ban
}

public record Comment(
    string ExternalId,
    string Author,
    string Text,
    DateTime CreatedAtUtc,
    bool IsHidden = false,
    string? ReplyToExternalId = null);