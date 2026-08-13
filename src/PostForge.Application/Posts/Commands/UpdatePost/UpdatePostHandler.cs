using ECO.Data;
using Mediator;
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

        var updateResult = post.UpdateText(request.Text);
        if (!updateResult.Success)
            throw new InvalidOperationException(
                string.Join("; ", updateResult.Errors.Select(e => $"{e.Context}: {e.Description}")));

        if (request.TargetPlatforms is not null)
        {
            foreach (var platform in request.TargetPlatforms)
            {
                var result = post.ScheduleForPlatform(platform);
                if (!result.Success)
                    throw new InvalidOperationException(
                        string.Join("; ", result.Errors.Select(e => $"{e.Context}: {e.Description}")));
            }
        }

        if (request.MediaAssets is not null)
        {
            foreach (var media in post.MediaAssets.ToList())
                post.RemoveMedia(media);

            foreach (var media in request.MediaAssets)
            {
                var result = post.AddMedia(media);
                if (!result.Success)
                    throw new InvalidOperationException(
                        string.Join("; ", result.Errors.Select(e => $"{e.Context}: {e.Description}")));
            }
        }

        if (request.Tags is not null)
        {
            var tags = new List<PostTag>(request.Tags.Count);
            foreach (var tagDto in request.Tags)
            {
                var tag = PostTag.Create(tagDto.Platform, tagDto.TagType, tagDto.Username);
                if (!tag.Success)
                    throw new InvalidOperationException(
                        string.Join("; ", tag.Errors.Select(e => $"{e.Context}: {e.Description}")));
                tags.Add(tag.Value!);
            }

            var setTagsResult = post.SetTags(tags);
            if (!setTagsResult.Success)
                throw new InvalidOperationException(
                    string.Join("; ", setTagsResult.Errors.Select(e => $"{e.Context}: {e.Description}")));
        }

        postRepository.Update(post);
        await dataContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
