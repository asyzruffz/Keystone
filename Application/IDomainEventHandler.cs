using Keystone.Core;

namespace Keystone.Application;

public interface IDomainEventHandler<in TDomainEvent> where TDomainEvent : IDomainEvent
{
    ValueTask Handle(TDomainEvent domainEvent, CancellationToken ct);
}
