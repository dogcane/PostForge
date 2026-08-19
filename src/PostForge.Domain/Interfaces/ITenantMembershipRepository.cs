using ECO;
using PostForge.Domain.Entities;

namespace PostForge.Domain.Interfaces;

public interface ITenantMembershipRepository : IRepository<TenantMembership, Guid>
{
}