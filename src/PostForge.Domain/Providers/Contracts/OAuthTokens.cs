namespace PostForge.Domain.Providers.Contracts;

public record OAuthTokens(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc);