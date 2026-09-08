using Domain.Common;

namespace Domain.Users.Events;

public sealed record UserCreatedDomainEvent(Guid userId) : DomainEvent;

