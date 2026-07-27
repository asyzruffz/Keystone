namespace Keystone.Application;

public interface IOperationHandler<in TOperation> where TOperation : IOperation
{
    ValueTask<Result> Handle(TOperation request, CancellationToken ct);
}

public interface IOperationHandler<in TOperation, TResponse> where TOperation : IOperation<TResponse>
{
    ValueTask<Result<TResponse>> Handle(TOperation request, CancellationToken ct);
}
