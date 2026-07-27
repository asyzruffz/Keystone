namespace Keystone.Application;

public interface ISender
{
    ValueTask<Result> Send<TOperation>(TOperation operation, CancellationToken ct = default)
        where TOperation : IOperation;
    ValueTask<Result<TResponse>> Send<TResponse>(IOperation<TResponse> operation, CancellationToken ct = default);
}
