using Mediator;
using PostForge.Application.Posts.DTOs;
using PostForge.Domain.Entities;

namespace PostForge.Application.Posts.Commands.UpdatePost;

public record UpdatePostCommand(
    Guid Id,
    string Text,
    List<MediaAsset>? MediaAssets,
    List<string>? TargetPlatforms,
    List<PostTagDto>? Tags = null) : IRequest<Unit>;
