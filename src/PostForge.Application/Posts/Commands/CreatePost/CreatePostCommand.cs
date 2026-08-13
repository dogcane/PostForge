using Mediator;
using PostForge.Application.Posts.DTOs;

namespace PostForge.Application.Posts.Commands.CreatePost;

public record CreatePostCommand(
    string Text,
    List<Guid>? MediaAssetIds,
    List<string>? TargetPlatforms,
    Guid? CampaignId,
    List<PostTagDto>? Tags = null) : IRequest<Guid>;
