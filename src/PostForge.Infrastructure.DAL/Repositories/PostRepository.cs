using ECO.Data;
using Microsoft.EntityFrameworkCore;
using PostForge.Domain.Entities;
using PostForge.Domain.Interfaces;

namespace PostForge.Infrastructure.DAL.Repositories;

public class PostRepository : TenantScopedRepository<Post, Guid>, IPostRepository
{
    public PostRepository(IDataContext dataContext, ITenantContext tenantContext) : base(dataContext, tenantContext) { }

    public async Task<List<MediaAsset>> GetMediaAssetsByIdsAsync(List<Guid> ids, CancellationToken ct)
    {
        return await DbContext.Set<MediaAsset>()
            .Where(m => ids.Contains(m.Identity))
            .ToListAsync(ct);
    }
}
