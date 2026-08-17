namespace PostForge.Providers.Facebook;

public class FacebookProviderOptions
{
    public const string SectionName = "Providers:Facebook";

    public string AppId { get; set; } = string.Empty;

    public string AppSecret { get; set; } = string.Empty;

    public string RedirectUri { get; set; } = string.Empty;

    public string DefaultPageId { get; set; } = string.Empty;

    public string ApiVersion { get; set; } = "v26.0";

    public bool EnableAppSecretProof { get; set; }
}
