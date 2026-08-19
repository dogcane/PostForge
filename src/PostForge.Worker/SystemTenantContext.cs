using PostForge.Domain.Interfaces;

namespace PostForge.Worker;

public sealed class SystemTenantContext : ITenantContext
{
    public Guid? TenantId => null;
    public Guid? UserId => null;
    public bool IsSuperUser => false;
}