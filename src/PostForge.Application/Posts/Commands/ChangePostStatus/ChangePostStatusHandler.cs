using ECO.Data;
using Mediator;
using PostForge.Domain.Interfaces;

namespace PostForge.Application.Posts.Commands.ChangePostStatus;

public class ChangePostStatusHandler(
    IPostRepository postRepository,
    IDataContext dataContext) : IRequestHandler<ChangePostStatusCommand, Unit>
{
    public async ValueTask<Unit> Handle(ChangePostStatusCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.LoadAsync(request.PostId)
            ?? throw new KeyNotFoundException($"Post with Id {request.PostId} was not found.");

        var result = post.SetStatus(request.NewStatus);
        if (!result.Success)
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => $"{e.Context}: {e.Description}")));

        postRepository.Update(post);
        await dataContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
