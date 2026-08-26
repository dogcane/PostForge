using System.Text.Json.Serialization;

namespace PostForge.Providers.Facebook;

public class FacebookCredentialSettings
{
    [JsonPropertyName("appId")]
    public string? AppId { get; set; }

    [JsonPropertyName("appSecret")]
    public string? AppSecret { get; set; }

    [JsonPropertyName("redirectUri")]
    public string? RedirectUri { get; set; }

    [JsonPropertyName("defaultPageId")]
    public string? DefaultPageId { get; set; }

    [JsonPropertyName("apiVersion")]
    public string? ApiVersion { get; set; }

    [JsonPropertyName("enableAppSecretProof")]
    public bool? EnableAppSecretProof { get; set; }
}
