using Mediator;
using PostForge.Domain.ValueObjects;

namespace PostForge.Application.Posts.Commands.CreatePost;

public record CreatePostCommand(
    string Text,
    List<Guid>? MediaAssetIds,
    List<SocialPlatform>? TargetPlatforms,
    Guid? CampaignId) : IRequest<Guid>;
