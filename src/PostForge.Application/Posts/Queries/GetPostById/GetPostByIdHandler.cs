using AutoMapper;
using Mediator;
using PostForge.Domain.Interfaces;
using PostForge.Application.Posts.DTOs;

namespace PostForge.Application.Posts.Queries.GetPostById;

public class GetPostByIdHandler(
    IPostRepository postRepository,
    IMapper mapper) : IRequestHandler<GetPostByIdQuery, PostDto?>
{
    public async ValueTask<PostDto?> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await postRepository.LoadAsync(request.Id);

        return post is null ? null : mapper.Map<PostDto>(post);
    }
}
