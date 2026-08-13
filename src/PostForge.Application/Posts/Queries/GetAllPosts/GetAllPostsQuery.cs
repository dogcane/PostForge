using Mediator;
using PostForge.Application.Posts.DTOs;
using PostForge.Domain.ValueObjects;

namespace PostForge.Application.Posts.Queries.GetAllPosts;

public record GetAllPostsQuery(
    PostStatus? Status,
    string? Platform,
    DateTime? DateFrom,
    DateTime? DateTo) : IRequest<List<PostDto>>;
