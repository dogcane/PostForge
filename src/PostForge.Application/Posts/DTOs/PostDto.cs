using AutoMapper;
using PostForge.Application.Common.Mappings;
using PostForge.Domain.Entities;
using PostForge.Domain.ValueObjects;

namespace PostForge.Application.Posts.DTOs;

public class PostDto : IMapFrom<Post>
{
    public Guid Id { get; set; }
    public string Text { get; set; } = null!;
    public List<MediaAsset> MediaAssets { get; set; } = [];
    public List<string> TargetPlatforms { get; set; } = [];
    public List<PostTagDto> Tags { get; set; } = [];
    public Guid? CampaignId { get; set; }
    public PostStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Post, PostDto>();
        profile.CreateMap<PostTag, PostTagDto>();
    }
}
