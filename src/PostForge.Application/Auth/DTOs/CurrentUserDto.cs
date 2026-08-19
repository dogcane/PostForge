using PostForge.Application.Tenants.DTOs;

namespace PostForge.Application.Auth.DTOs;

public class CurrentUserDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public bool IsSuperUser { get; set; }
    public List<TenantDto> Tenants { get; set; } = [];
}