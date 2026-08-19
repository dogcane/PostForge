using ECO.Data;
using Mediator;
using PostForge.Application.Common.Extensions;
using PostForge.Domain.Interfaces;
using PostForge.Domain.ValueObjects;

namespace PostForge.Application.Posts.Commands.UpdatePost;

public class UpdatePostHandler(
    IPostRepository postRepository,
    IDataContext dataContext) : IRequestHandler<UpdatePostCommand, Unit>
{
    public async ValueTask<Unit> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.LoadAsync(request.Id)
            ?? throw new KeyNotFoundException($"Post with Id {request.Id} was not found.");

        post.UpdateText(request.Text).EnsureSuccess();

        if (request.TargetPlatforms is not null)
        {
            foreach (var platform in request.TargetPlatforms)
                post.ScheduleForPlatform(platform).EnsureSuccess();
        }

        if (request.MediaAssets is not null)
        {
            foreach (var media in post.MediaAssets.ToList())
                post.RemoveMedia(media);

            foreach (var media in request.MediaAssets)
                post.AddMedia(media).EnsureSuccess();
        }

        if (request.Tags is not null)
        {
            var tags = request.Tags
                .Select(t => PostTag.Create(t.Platform, t.TagType, t.Username).EnsureSuccess())
                .ToList();

            post.SetTags(tags).EnsureSuccess();
        }

        postRepository.Update(post);
        await dataContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}