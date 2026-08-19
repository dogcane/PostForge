namespace PostForge.Application.Tenants.DTOs;

public class TenantUserDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public DateTime JoinedAtUtc { get; set; }
}