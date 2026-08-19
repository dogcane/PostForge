using ECO.Data;
using Mediator;
using PostForge.Application.Common.Extensions;
using PostForge.Domain.Entities;
using PostForge.Domain.Interfaces;
using PostForge.Domain.ValueObjects;

namespace PostForge.Application.Posts.Commands.CreatePost;

public class CreatePostHandler(
    IPostRepository postRepository,
    IDataContext dataContext,
    ITenantContext tenantContext) : IRequestHandler<CreatePostCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId
            ?? throw new InvalidOperationException("A tenant context is required to create a post.");

        var post = Post.Create(request.Text, tenantId, request.CampaignId).EnsureSuccess();

        if (request.TargetPlatforms is not null)
        {
            foreach (var platform in request.TargetPlatforms)
                post.ScheduleForPlatform(platform).EnsureSuccess();
        }

        if (request.Tags is not null)
        {
            foreach (var tagDto in request.Tags)
            {
                var tag = PostTag.Create(tagDto.Platform, tagDto.TagType, tagDto.Username).EnsureSuccess();
                post.AddTag(tag).EnsureSuccess();
            }
        }

        if (request.MediaAssetIds is not null)
        {
            var mediaAssets = await postRepository.GetMediaAssetsByIdsAsync(request.MediaAssetIds, cancellationToken);

            foreach (var media in mediaAssets)
                post.AddMedia(media).EnsureSuccess();
        }

        postRepository.Add(post);
        await dataContext.SaveChangesAsync(cancellationToken);

        return post.Id;
    }
}