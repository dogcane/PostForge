using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using PostForge.Application.Common.Mappings;
using PostForge.Application.Posts.DTOs;
using PostForge.Domain.Entities;
using PostForge.Domain.ValueObjects;

namespace PostForge.UnitTests.Application;

public class PostDtoMappingTests
{
    private readonly IMapper _mapper;

    public PostDtoMappingTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void Map_PostToPostDto_ShouldMapTags()
    {
        var post = Post.Create("Test content").Value!;
        post.ScheduleForPlatform("FACEBOOK");
        post.AddTag(PostTag.Create("FACEBOOK", PostTagType.Collaborator, "silvia.neri").Value!);

        var dto = _mapper.Map<PostDto>(post);

        dto.Tags.Should().ContainSingle(t =>
            t.Platform == "FACEBOOK"
            && t.TagType == PostTagType.Collaborator
            && t.Username == "silvia.neri");
    }
}