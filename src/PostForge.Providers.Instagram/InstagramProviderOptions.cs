namespace PostForge.Providers.Instagram;

public class InstagramProviderOptions
{
    public const string SectionName = "Providers:Instagram";

    public string AppId { get; set; } = string.Empty;

    public string AppSecret { get; set; } = string.Empty;

    public string RedirectUri { get; set; } = string.Empty;

    public string DefaultInstagramUserId { get; set; } = string.Empty;

    public string ApiVersion { get; set; } = "v22.0";
}
