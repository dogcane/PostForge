using ECO;
using PostForge.Domain.ValueObjects;
using Resulz;
using Resulz.Validation;

namespace PostForge.Domain.Entities;

public class SocialAccount : AggregateRoot<Guid>
{
    public Guid Id => Identity;
    public Guid TenantId { get; private set; }
    public string Platform { get; private set; }
    public string DisplayName { get; private set; }
    public string OAuthTokens { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime LastRefreshedAtUtc { get; private set; }

    private SocialAccount() : base(Guid.NewGuid())
    {
        Platform = null!;
        DisplayName = null!;
        OAuthTokens = null!;
    }

    private SocialAccount(Guid tenantId, string platform, string displayName, string oauthTokens) : base(Guid.NewGuid())
    {
        TenantId = tenantId;
        Platform = platform;
        DisplayName = displayName;
        OAuthTokens = oauthTokens;
        CreatedAtUtc = DateTime.UtcNow;
        LastRefreshedAtUtc = DateTime.UtcNow;
    }

    public static OperationResult<SocialAccount> Create(Guid tenantId, string platform, string displayName, string oauthTokens)
    {
        var result = OperationResult.MakeSuccess();
        result
            .With(tenantId, "TenantId").Condition(v => v != Guid.Empty)
            .With(platform, "Platform").Required().StringLength(50)
            .With(displayName, "DisplayName").Required().StringLength(200)
            .With(oauthTokens, "OAuthTokens").Required();
        if (!result.Success)
            return result;
        return OperationResult<SocialAccount>.MakeSuccess(new SocialAccount(tenantId, platform, displayName, oauthTokens));
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
