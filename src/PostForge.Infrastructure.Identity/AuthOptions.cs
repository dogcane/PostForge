namespace PostForge.Infrastructure.Identity;

public class AuthOptions
{
    public string Issuer { get; set; } = "PostForge";
    public string Audience { get; set; } = "PostForge.Api";
    public string SecretKey { get; set; } = string.Empty;
    public int ExpiresInMinutes { get; set; } = 60;
}