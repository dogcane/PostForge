using Mediator;
using PostForge.Application.Common.Mappings;
using PostForge.Application.Posts.DTOs;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.Posts.Queries.GetAllPosts;

public class GetAllPostsHandler(
    IPostRepository postRepository) : IRequestHandler<GetAllPostsQuery, List<PostDto>>
{
    public ValueTask<List<PostDto>> Handle(GetAllPostsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Domain.Entities.Post> query = postRepository;

        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);

        if (request.DateFrom.HasValue)
            query = query.Where(p => p.CreatedAtUtc >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(p => p.CreatedAtUtc <= request.DateTo.Value);

        var posts = query.ToList();

        if (!string.IsNullOrWhiteSpace(request.Platform))
            posts = posts.Where(p => p.TargetPlatforms.Contains(request.Platform)).ToList();

        return ValueTask.FromResult(posts.Select(p => p.ToDto()).ToList());
    }
}
