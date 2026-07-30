using PostForge.Domain.ValueObjects;
using PostForge.Infrastructure.Dtos;

namespace PostForge.Infrastructure;

public interface ISocialPlatformProvider
{
    SocialPlatform Platform { get; }
    Task<OAuthTokensDto> ExchangeAuthorizationCodeAsync(string code, CancellationToken ct);
    Task<OAuthTokensDto> RefreshTokenAsync(OAuthTokensDto tokens, CancellationToken ct);
    Task<PublishResultDto> PublishAsync(PostContentDto content, OAuthTokensDto tokens, CancellationToken ct);
    Task<PostInsightsDto?> GetInsightsAsync(string externalPostId, OAuthTokensDto tokens, CancellationToken ct);
}
