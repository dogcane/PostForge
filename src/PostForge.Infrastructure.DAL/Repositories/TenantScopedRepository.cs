using ECO;
using ECO.Data;
using ECO.Providers.EntityFramework;
using PostForge.Domain.Interfaces;

namespace PostForge.Infrastructure.DAL.Repositories;

public abstract class TenantScopedRepository<TEntity, TKey> : EntityFrameworkRepository<TEntity, TKey>
    where TEntity : class, IAggregateRoot<TKey>
    where TKey : IEquatable<TKey>
{
    protected TenantScopedRepository(IDataContext dataContext, ITenantContext tenantContext)
        : base(dataContext)
    {
        if (DbContext is PostForgeDbContext postForgeDbContext)
            postForgeDbContext.CurrentTenantId = tenantContext.TenantId;
    }
}