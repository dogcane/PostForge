using Mediator;

namespace PostForge.Application.Posts.Commands.DeletePost;

public record DeletePostCommand(Guid Id) : IRequest<Unit>;
