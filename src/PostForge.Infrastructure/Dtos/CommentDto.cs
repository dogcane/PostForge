namespace PostForge.Infrastructure.Dtos;

public enum CommentModerationAction
{
    Hide,
    Unhide,
    Delete,
    Ban
}

public record CommentDto(
    string ExternalId,
    string Author,
    string Text,
    DateTime CreatedAtUtc,
    bool IsHidden = false,
    string? ReplyToExternalId = null);
