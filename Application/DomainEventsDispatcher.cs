using Keystone.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Keystone.Application;

internal sealed class DomainEventsDispatcher : IDomainEventsDispatcher
{
    static readonly ConcurrentDictionary<Type, Type> handlerTypeDictionary = new();
    static readonly ConcurrentDictionary<Type, Type> wrapperTypeDictionary = new();

    readonly IServiceProvider provider;

    public DomainEventsDispatcher(IServiceProvider serviceProvider)
    {
        provider = serviceProvider;
    }

    public async ValueTask DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            using var scope = provider.CreateScope();

            Type domainEventType = domainEvent.GetType();
            Type handlerType = handlerTypeDictionary.GetOrAdd(domainEventType,
                et => typeof(IDomainEventHandler<>).MakeGenericType(et));

            IEnumerable<object?> handlers = scope.ServiceProvider.GetServices(handlerType);

            foreach (object? handler in handlers)
            {
                if (handler is null) continue;

                var handlerWrapper = HandlerWrapper.Create(handler, domainEventType);

                await handlerWrapper.Handle(domainEvent, ct);
            }
        }
    }

    private abstract class HandlerWrapper
    {
        public abstract ValueTask Handle(IDomainEvent domainEvent, CancellationToken ct);

        public static HandlerWrapper Create(object handler, Type domainEventType)
        {
            Type wrapperType = wrapperTypeDictionary.GetOrAdd(domainEventType,
                et => typeof(HandlerWrapper<>).MakeGenericType(et));
            return (HandlerWrapper)Activator.CreateInstance(wrapperType, handler)!;
        }
    }

    private sealed class HandlerWrapper<TDomainEvent> : HandlerWrapper where TDomainEvent : IDomainEvent
    {
        private readonly IDomainEventHandler<TDomainEvent> handler;

        public HandlerWrapper(object handler)
        {
            this.handler = (IDomainEventHandler<TDomainEvent>)handler;
        }

        public override ValueTask Handle(IDomainEvent domainEvent, CancellationToken ct)
        {
            return handler.Handle((TDomainEvent)domainEvent, ct);
        }
    }
}
