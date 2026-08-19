using Mediator;
using PostForge.Application.Common.Mappings;
using PostForge.Application.Posts.DTOs;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.Posts.Queries.GetPostById;

public class GetPostByIdHandler(
    IPostRepository postRepository) : IRequestHandler<GetPostByIdQuery, PostDto?>
{
    public async ValueTask<PostDto?> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await postRepository.LoadAsync(request.Id);

        return post is null ? null : post.ToDto();
    }
}
