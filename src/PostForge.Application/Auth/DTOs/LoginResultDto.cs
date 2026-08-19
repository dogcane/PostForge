namespace PostForge.Application.Auth.DTOs;

public class LoginResultDto
{
    public string Token { get; set; } = null!;
    public DateTime ExpiresAtUtc { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public bool IsSuperUser { get; set; }
}