namespace PostForge.Domain.Interfaces;

public interface ITenantContext
{
    Guid? TenantId { get; }
    Guid? UserId { get; }
    bool IsSuperUser { get; }
}