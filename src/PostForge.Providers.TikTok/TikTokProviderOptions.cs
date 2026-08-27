namespace PostForge.Providers.TikTok;

public class TikTokProviderOptions
{
    public const string SectionName = "Providers:TikTok";

    public string ClientKey { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string RedirectUri { get; set; } = string.Empty;

    public string ApiVersion { get; set; } = "v2";
}
