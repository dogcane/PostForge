using PostForge.Domain.ValueObjects;
using PostForge.Infrastructure.Dtos;

namespace PostForge.Infrastructure.Providers.Social;

public class TikTokProvider : ISocialPlatformProvider
{
    public SocialPlatform Platform => SocialPlatform.TikTok;

    public Task<OAuthTokensDto> ExchangeAuthorizationCodeAsync(string code, CancellationToken ct)
        => throw new NotImplementedException("TikTok OAuth code exchange will be implemented in Phase 4.");

    public Task<OAuthTokensDto> RefreshTokenAsync(OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotImplementedException("TikTok token refresh will be implemented in Phase 4.");

    public Task<PublishResultDto> PublishAsync(PostContentDto content, OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotImplementedException("TikTok post publishing will be implemented in Phase 4.");

    public Task<PostInsightsDto?> GetInsightsAsync(string externalPostId, OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotImplementedException("TikTok insights will be implemented in Phase 4.");
}
