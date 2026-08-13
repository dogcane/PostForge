namespace PostForge.Domain.Providers.Contracts;

public record CaptionRequest(string Brief, string? PlatformIdentifier = null, string? Tone = null);