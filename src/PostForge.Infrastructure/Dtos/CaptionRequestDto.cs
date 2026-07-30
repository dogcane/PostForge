using PostForge.Domain.ValueObjects;

namespace PostForge.Infrastructure.Dtos;

public record CaptionRequestDto(string Brief, SocialPlatform? Platform = null, string? Tone = null);
