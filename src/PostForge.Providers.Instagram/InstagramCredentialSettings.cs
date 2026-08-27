using System.Text.Json.Serialization;

namespace PostForge.Providers.Instagram;

public class InstagramCredentialSettings
{
    [JsonPropertyName("appId")]
    public string? AppId { get; set; }

    [JsonPropertyName("appSecret")]
    public string? AppSecret { get; set; }

    [JsonPropertyName("redirectUri")]
    public string? RedirectUri { get; set; }

    [JsonPropertyName("defaultInstagramUserId")]
    public string? DefaultInstagramUserId { get; set; }

    [JsonPropertyName("apiVersion")]
    public string? ApiVersion { get; set; }
}
