using System.Text.Json.Serialization;

namespace PostForge.Providers.TikTok;

public class TikTokCredentialSettings
{
    [JsonPropertyName("clientKey")]
    public string? ClientKey { get; set; }

    [JsonPropertyName("clientSecret")]
    public string? ClientSecret { get; set; }

    [JsonPropertyName("redirectUri")]
    public string? RedirectUri { get; set; }

    [JsonPropertyName("apiVersion")]
    public string? ApiVersion { get; set; }
}
