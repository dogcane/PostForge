using PostForge.Domain.ValueObjects;
using PostForge.Infrastructure.Dtos;

namespace PostForge.Infrastructure.Providers.Social;

public class YouTubeProvider : ISocialPlatformProvider
{
    public SocialPlatform Platform => SocialPlatform.YouTube;

    public Task<OAuthTokensDto> ExchangeAuthorizationCodeAsync(string code, CancellationToken ct)
        => throw new NotImplementedException("YouTube OAuth code exchange will be implemented in Phase 4.");

    public Task<OAuthTokensDto> RefreshTokenAsync(OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotImplementedException("YouTube token refresh will be implemented in Phase 4.");

    public Task<PublishResultDto> PublishAsync(PostContentDto content, OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotImplementedException("YouTube video upload will be implemented in Phase 4.");

    public Task<PostInsightsDto?> GetInsightsAsync(string externalPostId, OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotImplementedException("YouTube insights will be implemented in Phase 4.");
}
