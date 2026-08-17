using Resulz;
using Resulz.Validation;

namespace PostForge.Domain.ValueObjects;

public sealed class PostTag : ValueObject
{
    public string Platform { get; }
    public PostTagType TagType { get; }
    public string Username { get; }

    private PostTag(string platform, PostTagType tagType, string username)
    {
        Platform = platform;
        TagType = tagType;
        Username = username;
    }

    public static OperationResult<PostTag> Create(string platform, PostTagType tagType, string username)
    {
        var result = OperationResult.MakeSuccess();
        result
            .With(platform, "Platform").Required().StringLength(50)
            .With(tagType, "TagType").Condition(v => Enum.IsDefined(typeof(PostTagType), v))
            .With(username, "Username").Required().StringLength(200);
        if (!result.Success)
            return result;
        return OperationResult<PostTag>.MakeSuccess(new PostTag(platform, tagType, username));
    }

    protected override IEnumerable<object> GetEqualityComponents() => [Platform, TagType, Username];
}