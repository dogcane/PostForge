using ECO.Data;
using ECO.Providers.EntityFramework;
using Microsoft.EntityFrameworkCore;
using PostForge.Domain.Interfaces;
using PostForge.Domain.Entities;

namespace PostForge.Infrastructure.DAL.Repositories;

public class PostRepository : EntityFrameworkRepository<Post, Guid>, IPostRepository
{
    public PostRepository(IDataContext dataContext) : base(dataContext) { }

    public async Task<List<MediaAsset>> GetMediaAssetsByIdsAsync(List<Guid> ids, CancellationToken ct)
    {
        return await DbContext.Set<MediaAsset>()
            .Where(m => ids.Contains(m.Identity))
            .ToListAsync(ct);
    }
}
