namespace Keystone;

public class Result
{
    public bool IsSuccess { get; init; }
    public string ErrorMessage { get; init; }

    private Result(bool isSuccess, string errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static Result Ok() => new Result(true, string.Empty);
    public static Result Fail(string errorMessage) => new Result(false, errorMessage);

    public Result OnError(Action<string> action) { if (!IsSuccess) action(ErrorMessage); return this; }
    public Result FixError(Func<string, Result> action) =>
        !IsSuccess ? action(ErrorMessage) : this;

    public Result Then(Func<Result> action) =>
        IsSuccess ? action() : this;
    public Result<TResult> Then<TResult>(Func<Result<TResult>> action) =>
        IsSuccess ? action() : Result<TResult>.Fail(ErrorMessage);
    public void Then(Action action) { if (IsSuccess) action(); }

    public Task<Result> ThenAsync(Func<CancellationToken, Task<Result>> action, CancellationToken ct = default) =>
        IsSuccess ? action(ct) : Task.FromResult(this);
    public Task<Result<TResult>> ThenAsync<TResult>(Func<CancellationToken, Task<Result<TResult>>> action, CancellationToken ct = default) =>
        IsSuccess ? action(ct) : Task.FromResult(Result<TResult>.Fail(ErrorMessage));
    public Task ThenAsync(Func<CancellationToken, Task> action, CancellationToken ct = default) => IsSuccess ? action(ct) : Task.CompletedTask;

    public Result<TResult> Map<TResult>(Func<TResult> action) =>
        Then(() => Result<TResult>.Ok(action()));

    public Task<Result<TResult>> MapAsync<TResult>(
        Func<CancellationToken, Task<TResult>> action,
        CancellationToken ct = default) =>
        ThenAsync(async ct => Result<TResult>.Ok(await action(ct)), ct);

    public TResult Match<TResult>(Func<TResult> onSuccess, Func<string, TResult> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(ErrorMessage);

    public Task MatchAsync(
        Func<Task> onSuccess,
        Func<string, Task> onFailure,
        CancellationToken ct = default) =>
        IsSuccess ? onSuccess() : onFailure(ErrorMessage);
    public Task<TResult> MatchAsync<TResult>(
        Func<Task<TResult>> onSuccess,
        Func<string, Task<TResult>> onFailure,
        CancellationToken ct = default) =>
        IsSuccess ? onSuccess() : onFailure(ErrorMessage);
}

public class Result<T>
{
    public bool IsSuccess { get; init; }
    public string ErrorMessage { get; init; }

    private T Value;

    private Result(bool isSuccess, T value, string errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Ok(T value) => new Result<T>(true, value, string.Empty);
    public static Result<T> Fail(string errorMessage) => new Result<T>(false, default!, errorMessage);

    public T Or(T defaultVal) => IsSuccess ? Value : defaultVal;

    public Result<T> OnError(Action<string> action) { if (!IsSuccess) action(ErrorMessage); return this; }
    public Result<T> FixError(Func<string, Result<T>> action) =>
        !IsSuccess ? action(ErrorMessage) : this;

    public Result Then(Func<T, Result> action) =>
        IsSuccess ? action(Value) : Result.Fail(ErrorMessage);
    public Result<TResult> Then<TResult>(Func<T, Result<TResult>> action) =>
        IsSuccess ? action(Value) : Result<TResult>.Fail(ErrorMessage);
    public void Then(Action<T> action) { if (IsSuccess) action(Value); }

    public Task<Result> ThenAsync(Func<T, CancellationToken, Task<Result>> action, CancellationToken ct = default) =>
        IsSuccess ? action(Value, ct) : Task.FromResult(Result.Fail(ErrorMessage));
    public Task<Result<TResult>> ThenAsync<TResult>(Func<T, CancellationToken, Task<Result<TResult>>> action, CancellationToken ct = default) =>
        IsSuccess ? action(Value, ct) : Task.FromResult(Result<TResult>.Fail(ErrorMessage));
    public Task ThenAsync(Func<T, CancellationToken, Task> action, CancellationToken ct = default) => IsSuccess ? action(Value, ct) : Task.CompletedTask;

    public Result<TResult> Map<TResult>(Func<T, TResult> action) =>
        Then(v => Result<TResult>.Ok(action(v)));

    public Task<Result<TResult>> MapAsync<TResult>(
        Func<T, CancellationToken, Task<TResult>> action,
        CancellationToken ct = default) =>
        ThenAsync(async (v, ct) => Result<TResult>.Ok(await action(v, ct)), ct);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(ErrorMessage);

    public Task MatchAsync(
        Func<T, CancellationToken, Task> onSuccess,
        Func<string, CancellationToken, Task> onFailure,
        CancellationToken ct = default) =>
        IsSuccess ? onSuccess(Value, ct) : onFailure(ErrorMessage, ct);
    public Task<TResult> MatchAsync<TResult>(
        Func<T, CancellationToken, Task<TResult>> onSuccess,
        Func<string, CancellationToken, Task<TResult>> onFailure,
        CancellationToken ct = default) =>
        IsSuccess ? onSuccess(Value, ct) : onFailure(ErrorMessage, ct);
}

public static class ResultExtensions
{
    public static Option<T> ToOption<T>(this Result<T> result) =>
        result.Match(Option<T>.Some, _ => Option<T>.None());
}
