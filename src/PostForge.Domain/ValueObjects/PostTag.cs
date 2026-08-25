using Resulz;
using Resulz.Validation;

namespace PostForge.Domain.ValueObjects;

public sealed class PostTag : ValueObject
{
    #region Properties
    public string Platform { get; }
    public PostTagType TagType { get; }
    public string Username { get; }
    #endregion

    #region ctor
    private PostTag(string platform, PostTagType tagType, string username)
    {
        Platform = platform;
        TagType = tagType;
        Username = username;
    }
    #endregion

    #region Methods
    private static OperationResult Validate(string platform, PostTagType tagType, string username)
    {
        var result = OperationResult.MakeSuccess();
        result
            .With(platform, "Platform").Required().StringLength(50)
            .With(tagType, "TagType").Condition(v => Enum.IsDefined(v))
            .With(username, "Username").Required().StringLength(200);
        return result;
    }

    public static OperationResult<PostTag> Create(string platform, PostTagType tagType, string username)
        => Validate(platform, tagType, username)
            .IfSuccessThenReturn<PostTag>(() => new PostTag(platform, tagType, username));

    protected override IEnumerable<object> GetEqualityComponents() => [Platform, TagType, Username];
    #endregion
}
