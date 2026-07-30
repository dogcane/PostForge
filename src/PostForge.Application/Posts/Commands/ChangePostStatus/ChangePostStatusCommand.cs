using Mediator;
using PostForge.Domain.ValueObjects;

namespace PostForge.Application.Posts.Commands.ChangePostStatus;

public record ChangePostStatusCommand(Guid PostId, PostStatus NewStatus) : IRequest<Unit>;
