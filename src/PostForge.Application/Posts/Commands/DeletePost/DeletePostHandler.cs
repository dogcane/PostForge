using ECO.Data;
using Mediator;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.Posts.Commands.DeletePost;

public class DeletePostHandler(
    IPostRepository postRepository,
    IDataContext dataContext) : IRequestHandler<DeletePostCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.LoadAsync(request.Id)
            ?? throw new KeyNotFoundException($"Post with Id {request.Id} was not found.");

        postRepository.Remove(post);
        await dataContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
