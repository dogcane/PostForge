using PostForge.Domain.Entities;
using PostForge.Domain.Providers.Contracts;
using PostForge.Domain.ValueObjects;

namespace PostForge.Application.Publishing;

public static class PostPublishSettingsMapper
{
    public static PublishSettings ToPublishSettings(this Post post, string platform)
    {
        var platformTags = post.Tags
            .Where(t => t.Platform == platform)
            .ToList();

        return new PublishSettings(
            MentionedUsernames: platformTags
                .Where(t => t.TagType == PostTagType.Mention)
                .Select(t => t.Username)
                .ToList(),
            UserTagUsernames: platformTags
                .Where(t => t.TagType == PostTagType.UserTag)
                .Select(t => t.Username)
                .ToList(),
            CollaboratorUsernames: platformTags
                .Where(t => t.TagType == PostTagType.Collaborator)
                .Select(t => t.Username)
                .ToList());
    }
}