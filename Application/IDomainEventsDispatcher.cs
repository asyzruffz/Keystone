using Keystone.Core;

namespace Keystone.Application;

public interface IDomainEventsDispatcher
{
    ValueTask DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default);
}
