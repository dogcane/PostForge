using AutoMapper;
using Mediator;
using PostForge.Domain.Interfaces;
using PostForge.Application.Posts.DTOs;

namespace PostForge.Application.Posts.Queries.GetAllPosts;

public class GetAllPostsHandler(
    IPostRepository postRepository,
    IMapper mapper) : IRequestHandler<GetAllPostsQuery, List<PostDto>>
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

        if (request.Platform.HasValue)
            posts = posts.Where(p => p.TargetPlatforms.Contains(request.Platform.Value)).ToList();

        return ValueTask.FromResult(mapper.Map<List<PostDto>>(posts));
    }
}
