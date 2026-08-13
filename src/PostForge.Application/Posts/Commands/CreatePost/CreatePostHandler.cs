using ECO.Data;
using Mediator;
using PostForge.Domain.Interfaces;
using PostForge.Domain.Entities;
using PostForge.Domain.ValueObjects;

namespace PostForge.Application.Posts.Commands.CreatePost;

public class CreatePostHandler(
    IPostRepository postRepository,
    IDataContext dataContext) : IRequestHandler<CreatePostCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var postResult = Post.Create(request.Text, request.CampaignId);
        if (!postResult.Success)
            throw new InvalidOperationException(
                string.Join("; ", postResult.Errors.Select(e => $"{e.Context}: {e.Description}")));

        var post = postResult.Value!;

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

        if (request.Tags is not null)
        {
            foreach (var tagDto in request.Tags)
            {
                var tag = PostTag.Create(tagDto.Platform, tagDto.TagType, tagDto.Username);
                if (!tag.Success)
                    throw new InvalidOperationException(
                        string.Join("; ", tag.Errors.Select(e => $"{e.Context}: {e.Description}")));

                var result = post.AddTag(tag.Value!);
                if (!result.Success)
                    throw new InvalidOperationException(
                        string.Join("; ", result.Errors.Select(e => $"{e.Context}: {e.Description}")));
            }
        }

        if (request.MediaAssetIds is not null)
        {
            var mediaAssets = await postRepository.GetMediaAssetsByIdsAsync(request.MediaAssetIds, cancellationToken);

            foreach (var media in mediaAssets)
            {
                var result = post.AddMedia(media);
                if (!result.Success)
                    throw new InvalidOperationException(
                        string.Join("; ", result.Errors.Select(e => $"{e.Context}: {e.Description}")));
            }
        }

        postRepository.Add(post);
        await dataContext.SaveChangesAsync(cancellationToken);

        return post.Id;
    }
}
