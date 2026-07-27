using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Keystone.Application;

internal class Sender : ISender
{
    static readonly ConcurrentDictionary<Type, HandlerWrapperBase> handlerDictionary = new();

    readonly IServiceProvider provider;

    public Sender(IServiceProvider serviceProvider)
    {
        provider = serviceProvider;
    }

    public ValueTask<Result> Send<TOperation>(TOperation operation, CancellationToken ct = default)
        where TOperation : IOperation
    {
        var handler = provider.GetRequiredService<IOperationHandler<TOperation>>();
        return handler.Handle(operation, ct);
    }

    public async ValueTask<Result<TResponse>> Send<TResponse>(IOperation<TResponse> operation, CancellationToken ct = default)
    {
        Type operationType = operation.GetType();
        Type responseType = typeof(TResponse);
        var handler = handlerDictionary.GetOrAdd(operationType, ot =>
        {
            var wrapperType = typeof(HandlerWrapper<,>).MakeGenericType(ot, responseType);
            var wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper type for {ot}");
            return (HandlerWrapperBase)wrapper;
        });

        return (Result<TResponse>)(await handler.Handle(operation, provider, ct).ConfigureAwait(false))!;
    }

    private abstract class HandlerWrapperBase
    {
        public abstract ValueTask<object?> Handle(object operation, IServiceProvider provider,
            CancellationToken ct);
    }

    private class HandlerWrapper<TOperation, TResponse> : HandlerWrapperBase
        where TOperation : IOperation<TResponse>
    {
        public override async ValueTask<object?> Handle(object operation, IServiceProvider provider,
            CancellationToken ct)
        {
            return await Handle((IOperation<TResponse>)operation, provider, ct).ConfigureAwait(false);
        }

        public ValueTask<Result<TResponse>> Handle(IOperation<TResponse> operation, IServiceProvider provider,
            CancellationToken ct)
        {
            var handler = provider.GetRequiredService<IOperationHandler<TOperation, TResponse>>();
            return handler.Handle((TOperation)operation, ct);
        }
    }
}
