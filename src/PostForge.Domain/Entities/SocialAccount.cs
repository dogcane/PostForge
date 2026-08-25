using ECO;
using Resulz;
using Resulz.Validation;

namespace PostForge.Domain.Entities;

public class SocialAccount : AggregateRoot<Guid>
{
    #region Fields
    #endregion

    #region Properties
    public Guid Id => Identity;
    public Guid TenantId { get; private set; }
    public string Platform { get; private set; }
    public string DisplayName { get; private set; }
    public string OAuthTokens { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime LastRefreshedAtUtc { get; private set; }
    #endregion

    #region ctor
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
    #endregion

    #region Methods
    protected static OperationResult Validate(Guid tenantId, string platform, string displayName, string oauthTokens)
    {
        var result = OperationResult.MakeSuccess();
        result
            .With(tenantId, "TenantId").Condition(v => v != Guid.Empty)
            .With(platform, "Platform").Required().StringLength(50)
            .With(displayName, "DisplayName").Required().StringLength(200)
            .With(oauthTokens, "OAuthTokens").Required();
        return result;
    }

    public static OperationResult<SocialAccount> Create(Guid tenantId, string platform, string displayName, string oauthTokens)
        => Validate(tenantId, platform, displayName, oauthTokens)
            .IfSuccessThenReturn<SocialAccount>(() => new SocialAccount(tenantId, platform, displayName, oauthTokens));

    public OperationResult RefreshTokens(string oauthTokens)
        => OperationResult.MakeSuccess()
            .With(oauthTokens, "OAuthTokens").Required()
            .Result
            .IfSuccess(_ =>
            {
                OAuthTokens = oauthTokens;
                LastRefreshedAtUtc = DateTime.UtcNow;
            });
    #endregion
}
