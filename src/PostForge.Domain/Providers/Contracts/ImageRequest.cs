namespace PostForge.Domain.Providers.Contracts;

public record ImageRequest(string Prompt, string? Style = null);