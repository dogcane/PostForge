using System.Text.Json.Serialization;

namespace PostForge.Providers.YouTube;

public class YouTubeCredentialSettings
{
    [JsonPropertyName("clientId")]
    public string? ClientId { get; set; }

    [JsonPropertyName("clientSecret")]
    public string? ClientSecret { get; set; }

    [JsonPropertyName("redirectUri")]
    public string? RedirectUri { get; set; }

    [JsonPropertyName("apiVersion")]
    public string? ApiVersion { get; set; }
}
