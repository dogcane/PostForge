namespace PostForge.Infrastructure.Identity;

public class AuthOptions
{
    public string Issuer { get; set; } = "PostForge";
    public string Audience { get; set; } = "PostForge.Api";
    public string SecretKey { get; set; } = string.Empty;
    public int ExpiresInMinutes { get; set; } = 60;
    public int RefreshTokenExpiresInDays { get; set; } = 7;
    public int RefreshTokenLengthBytes { get; set; } = 64;
    public SuperUserOptions? SuperUser { get; set; }
}

public class SuperUserOptions
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}