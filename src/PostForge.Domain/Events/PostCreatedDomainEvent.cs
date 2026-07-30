using ECO.Events;

namespace PostForge.Domain.Events;

public sealed record PostCreatedDomainEvent(Guid PostId, DateTime CreatedAtUtc) : IDomainEvent;
