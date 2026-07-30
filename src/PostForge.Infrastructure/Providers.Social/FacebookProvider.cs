using PostForge.Domain.ValueObjects;
using PostForge.Infrastructure.Dtos;

namespace PostForge.Infrastructure.Providers.Social;

public class FacebookProvider : ISocialPlatformProvider
{
    public SocialPlatform Platform => SocialPlatform.Facebook;

    public Task<OAuthTokensDto> ExchangeAuthorizationCodeAsync(string code, CancellationToken ct)
        => throw new NotImplementedException("Facebook OAuth code exchange will be implemented in Phase 1.");

    public Task<OAuthTokensDto> RefreshTokenAsync(OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotImplementedException("Facebook token refresh will be implemented in Phase 1.");

    public Task<PublishResultDto> PublishAsync(PostContentDto content, OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotImplementedException("Facebook post publishing will be implemented in Phase 1.");

    public Task<PostInsightsDto?> GetInsightsAsync(string externalPostId, OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotImplementedException("Facebook insights will be implemented in Phase 1.");
}
