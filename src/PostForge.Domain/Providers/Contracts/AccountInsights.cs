namespace PostForge.Domain.Providers.Contracts;

public record AccountInsights(
    long? FollowerCount = null,
    long? Impressions = null,
    long? Reach = null,
    long? ProfileViews = null,
    double? EngagementRate = null);