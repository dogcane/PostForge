using ECO.Data;
using Microsoft.EntityFrameworkCore;
using PostForge.Domain.Entities;
using PostForge.Domain.Interfaces;

namespace PostForge.Infrastructure.DAL.Repositories;

public class PostRepository(IDataContext dataContext, ITenantContext tenantContext)
    : TenantScopedRepository<Post, Guid>(dataContext, tenantContext), IPostRepository
{
    public async Task<List<MediaAsset>> GetMediaAssetsByIdsAsync(List<Guid> ids, CancellationToken ct)
    {
        return await DbContext.Set<MediaAsset>()
            .Where(m => ids.Contains(m.Identity))
            .ToListAsync(ct);
    }
}
