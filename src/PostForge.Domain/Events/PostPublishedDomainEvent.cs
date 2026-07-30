using ECO.Events;
using PostForge.Domain.ValueObjects;

namespace PostForge.Domain.Events;

public sealed record PostPublishedDomainEvent(Guid PostId, SocialPlatform Platform, DateTime PublishedAtUtc) : IDomainEvent;
