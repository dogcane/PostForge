using Mediator;
using PostForge.Domain.Entities;
using PostForge.Domain.ValueObjects;

namespace PostForge.Application.Posts.Commands.UpdatePost;

public record UpdatePostCommand(
    Guid Id,
    string Text,
    List<MediaAsset>? MediaAssets,
    List<SocialPlatform>? TargetPlatforms) : IRequest<Unit>;
