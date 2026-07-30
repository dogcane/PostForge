using ECO.Events;
using PostForge.Domain.ValueObjects;

namespace PostForge.Domain.Events;

public sealed record PostStatusChangedDomainEvent(Guid PostId, PostStatus OldStatus, PostStatus NewStatus) : IDomainEvent;
