using PostForge.Domain.ValueObjects;

namespace PostForge.Application.Posts.DTOs;

public record PostTagDto(string Platform, PostTagType TagType, string Username);