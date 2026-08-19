using PostForge.Domain.Interfaces;

namespace PostForge.Infrastructure.Identity.Tenancy;

public sealed class ApplicationTenantContext : ITenantContext
{
    public Guid? TenantId { get; set; }
    public Guid? UserId { get; set; }
    public bool IsSuperUser { get; set; }
}