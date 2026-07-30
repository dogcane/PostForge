using Mediator;
using PostForge.Application.Posts.DTOs;

namespace PostForge.Application.Posts.Queries.GetPostById;

public record GetPostByIdQuery(Guid Id) : IRequest<PostDto?>;
