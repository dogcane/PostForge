using ECO.Data;
using ECO.Providers.EntityFramework;
using PostForge.Domain.Entities;
using PostForge.Domain.Interfaces;

namespace PostForge.Infrastructure.DAL.Repositories;

public class TenantRepository : EntityFrameworkRepository<Tenant, Guid>, ITenantRepository
{
    public TenantRepository(IDataContext dataContext) : base(dataContext) { }
}