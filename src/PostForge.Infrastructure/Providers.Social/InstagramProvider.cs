using PostForge.Domain.ValueObjects;
using PostForge.Infrastructure.Dtos;

namespace PostForge.Infrastructure.Providers.Social;

public class InstagramProvider : ISocialPlatformProvider
{
    public SocialPlatform Platform => SocialPlatform.Instagram;

    public Task<OAuthTokensDto> ExchangeAuthorizationCodeAsync(string code, CancellationToken ct)
        => throw new NotImplementedException("Instagram OAuth code exchange will be implemented in Phase 1.");

    public Task<OAuthTokensDto> RefreshTokenAsync(OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotImplementedException("Instagram token refresh will be implemented in Phase 1.");

    public Task<PublishResultDto> PublishAsync(PostContentDto content, OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotImplementedException("Instagram post publishing will be implemented in Phase 1.");

    public Task<PostInsightsDto?> GetInsightsAsync(string externalPostId, OAuthTokensDto tokens, CancellationToken ct)
        => throw new NotImplementedException("Instagram insights will be implemented in Phase 1.");
}
