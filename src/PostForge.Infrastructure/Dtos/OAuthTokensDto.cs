namespace PostForge.Infrastructure.Dtos;

public record OAuthTokensDto(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc);
