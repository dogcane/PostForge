using ECO;
using PostForge.Domain.Entities;

namespace PostForge.Domain.Interfaces;

public interface ITenantRepository : IRepository<Tenant, Guid>
{
}