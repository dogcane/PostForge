using ECO;
using PostForge.Domain.ValueObjects;
using Resulz;
using Resulz.Validation;

namespace PostForge.Domain.Entities;

public class SocialAccount : AggregateRoot<Guid>
{
    public Guid Id => Identity;
    public SocialPlatform Platform { get; private set; }
    public string DisplayName { get; private set; }
    public string OAuthTokens { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime LastRefreshedAtUtc { get; private set; }

    private SocialAccount() : base(Guid.NewGuid())
    {
        DisplayName = null!;
        OAuthTokens = null!;
    }

    private SocialAccount(SocialPlatform platform, string displayName, string oauthTokens) : base(Guid.NewGuid())
    {
        Platform = platform;
        DisplayName = displayName;
        OAuthTokens = oauthTokens;
        CreatedAtUtc = DateTime.UtcNow;
        LastRefreshedAtUtc = DateTime.UtcNow;
    }

    public static OperationResult<SocialAccount> Create(SocialPlatform platform, string displayName, string oauthTokens)
    {
        var result = OperationResult.MakeSuccess();
        result
            .With(platform, "Platform").Condition(v => Enum.IsDefined(typeof(SocialPlatform), v))
            .With(displayName, "DisplayName").Required().StringLength(200)
            .With(oauthTokens, "OAuthTokens").Required();
        if (!result.Success)
            return result;
        return OperationResult<SocialAccount>.MakeSuccess(new SocialAccount(platform, displayName, oauthTokens));
    }

    public OperationResult RefreshTokens(string oauthTokens)
    {
        var result = OperationResult.MakeSuccess();
        result.With(oauthTokens, "OAuthTokens").Required();
        if (!result.Success)
            return result;
        OAuthTokens = oauthTokens;
        LastRefreshedAtUtc = DateTime.UtcNow;
        return OperationResult.MakeSuccess();
    }
}
