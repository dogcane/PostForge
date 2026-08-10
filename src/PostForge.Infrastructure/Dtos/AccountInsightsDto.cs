namespace PostForge.Infrastructure.Dtos;

public record AccountInsightsDto(
    long? FollowerCount = null,
    long? Impressions = null,
    long? Reach = null,
    long? ProfileViews = null,
    double? EngagementRate = null);
