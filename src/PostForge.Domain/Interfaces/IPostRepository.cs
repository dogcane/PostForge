using ECO;
using PostForge.Domain.Entities;

namespace PostForge.Domain.Interfaces;

public interface IPostRepository : IRepository<Post, Guid>
{
    Task<List<MediaAsset>> GetMediaAssetsByIdsAsync(List<Guid> ids, CancellationToken ct);
}
