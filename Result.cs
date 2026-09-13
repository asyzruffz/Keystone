namespace Keystone;

public record Result
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

    public void Then(Action action) { if (IsSuccess) action(); }
    public Result Then(Func<Result> action) =>
        IsSuccess ? action() : this;
    public Result<TResult> Then<TResult>(Func<Result<TResult>> action) =>
        IsSuccess ? action() : Result<TResult>.Fail(ErrorMessage);

    public ValueTask ThenAsync(Func<CancellationToken, ValueTask> action, CancellationToken ct = default) =>
        IsSuccess ? action(ct) : ValueTask.CompletedTask;
    public ValueTask<Result> ThenAsync(Func<CancellationToken, ValueTask<Result>> action, CancellationToken ct = default) =>
        IsSuccess ? action(ct) : ValueTask.FromResult(this);
    public ValueTask<Result<TResult>> ThenAsync<TResult>(Func<CancellationToken, ValueTask<Result<TResult>>> action, CancellationToken ct = default) =>
        IsSuccess ? action(ct) : ValueTask.FromResult(Result<TResult>.Fail(ErrorMessage));

    public Result<TResult> Map<TResult>(Func<TResult> action) =>
        Then(() => Result<TResult>.Ok(action()));

    public ValueTask<Result<TResult>> MapAsync<TResult>(
        Func<CancellationToken, ValueTask<TResult>> action,
        CancellationToken ct = default) =>
        ThenAsync(async ct => Result<TResult>.Ok(await action(ct).ConfigureAwait(false)), ct);

    public void Match(Action onSuccess, Action<string> onFailure)
    { if (IsSuccess) onSuccess(); else onFailure(ErrorMessage); }
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<string, TResult> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(ErrorMessage);

    public ValueTask MatchAsync(
        Func<ValueTask> onSuccess,
        Func<string, ValueTask> onFailure,
        CancellationToken ct = default) =>
        IsSuccess ? onSuccess() : onFailure(ErrorMessage);
    public ValueTask<TResult> MatchAsync<TResult>(
        Func<ValueTask<TResult>> onSuccess,
        Func<string, ValueTask<TResult>> onFailure,
        CancellationToken ct = default) =>
        IsSuccess ? onSuccess() : onFailure(ErrorMessage);
}

public record Result<T>
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

    public T Data => IsSuccess ? Value : Throw.InvalidOperation<T>("Invalid data in None");
    public T Or(T defaultVal) => IsSuccess ? Value : defaultVal;

    public Result<T> OnError(Action<string> action) { if (!IsSuccess) action(ErrorMessage); return this; }
    public Result<T> FixError(Func<string, Result<T>> action) =>
        !IsSuccess ? action(ErrorMessage) : this;

    public void Then(Action<T> action) { if (IsSuccess) action(Value); }
    public Result Then(Func<T, Result> action) =>
        IsSuccess ? action(Value) : Result.Fail(ErrorMessage);
    public Result<TResult> Then<TResult>(Func<T, Result<TResult>> action) =>
        IsSuccess ? action(Value) : Result<TResult>.Fail(ErrorMessage);

    public ValueTask ThenAsync(Func<T, CancellationToken, ValueTask> action, CancellationToken ct = default) =>
        IsSuccess ? action(Value, ct) : ValueTask.CompletedTask;
    public ValueTask<Result> ThenAsync(Func<T, CancellationToken, ValueTask<Result>> action, CancellationToken ct = default) =>
        IsSuccess ? action(Value, ct) : ValueTask.FromResult(Result.Fail(ErrorMessage));
    public ValueTask<Result<TResult>> ThenAsync<TResult>(Func<T, CancellationToken, ValueTask<Result<TResult>>> action, CancellationToken ct = default) =>
        IsSuccess ? action(Value, ct) : ValueTask.FromResult(Result<TResult>.Fail(ErrorMessage));

    public Result<TResult> Map<TResult>(Func<T, TResult> action) =>
        Then(v => Result<TResult>.Ok(action(v)));

    public ValueTask<Result<TResult>> MapAsync<TResult>(
        Func<T, CancellationToken, ValueTask<TResult>> action,
        CancellationToken ct = default) =>
        ThenAsync(async (v, ct) => Result<TResult>.Ok(await action(v, ct).ConfigureAwait(false)), ct);

    public void Match(Action<T> onSuccess, Action<string> onFailure)
    { if (IsSuccess) onSuccess(Value); else onFailure(ErrorMessage); }
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(ErrorMessage);

    public ValueTask MatchAsync(
        Func<T, CancellationToken, ValueTask> onSuccess,
        Func<string, CancellationToken, ValueTask> onFailure,
        CancellationToken ct = default) =>
        IsSuccess ? onSuccess(Value, ct) : onFailure(ErrorMessage, ct);
    public ValueTask<TResult> MatchAsync<TResult>(
        Func<T, CancellationToken, ValueTask<TResult>> onSuccess,
        Func<string, CancellationToken, ValueTask<TResult>> onFailure,
        CancellationToken ct = default) =>
        IsSuccess ? onSuccess(Value, ct) : onFailure(ErrorMessage, ct);
}

public static class ResultExtensions
{
    public static Option<T> ToOption<T>(this Result<T> result) =>
        result.Match(Option<T>.Some, _ => Option<T>.None());

    // ----------------

    public static async ValueTask<Result> OnError(this ValueTask<Result> result,
        Action<string> action) =>
        (await result.ConfigureAwait(false)).OnError(action);
    public static async ValueTask<Result> FixError(this ValueTask<Result> result,
        Func<string, Result> action) =>
        (await result.ConfigureAwait(false)).FixError(action);

    public static async ValueTask<Result> Then(this ValueTask<Result> result,
        Func<Result> action) =>
        (await result.ConfigureAwait(false)).Then(action);
    public static async ValueTask<Result<TResult>> Then<TResult>(this ValueTask<Result> result,
        Func<Result<TResult>> action) =>
        (await result.ConfigureAwait(false)).Then(action);
    public static async ValueTask Then(this ValueTask<Result> result, Action action) =>
        (await result.ConfigureAwait(false)).Then(action);

    public static async ValueTask ThenAsync(this ValueTask<Result> result,
        Func<CancellationToken, ValueTask> action, CancellationToken ct = default) =>
        await (await result.ConfigureAwait(false)).ThenAsync(action, ct).ConfigureAwait(false);
    public static async ValueTask<Result> ThenAsync(this ValueTask<Result> result,
        Func<CancellationToken, ValueTask<Result>> action, CancellationToken ct = default) =>
        await (await result.ConfigureAwait(false)).ThenAsync(action, ct).ConfigureAwait(false);
    public static async ValueTask<Result<TResult>> ThenAsync<TResult>(this ValueTask<Result> result,
        Func<CancellationToken, ValueTask<Result<TResult>>> action, CancellationToken ct = default) =>
        await (await result.ConfigureAwait(false)).ThenAsync(action, ct).ConfigureAwait(false);

    public static async ValueTask<Result<TResult>> Map<TResult>(this ValueTask<Result> result,
        Func<TResult> action) =>
        (await result.ConfigureAwait(false)).Map(action);
    public static async ValueTask<Result<TResult>> MapAsync<TResult>(this ValueTask<Result> result,
        Func<CancellationToken, ValueTask<TResult>> action,
        CancellationToken ct = default) =>
        await (await result.ConfigureAwait(false)).MapAsync(action, ct).ConfigureAwait(false);

    public static async ValueTask Match(this ValueTask<Result> result,
        Action onSuccess, Action<string> onFailure) =>
        (await result.ConfigureAwait(false)).Match(onSuccess, onFailure);
    public static async ValueTask<TResult> Match<TResult>(this ValueTask<Result> result,
        Func<TResult> onSuccess,
        Func<string, TResult> onFailure) =>
        (await result.ConfigureAwait(false)).Match(onSuccess, onFailure);

    public static async ValueTask MatchAsync(this ValueTask<Result> result,
        Func<ValueTask> onSuccess,
        Func<string, ValueTask> onFailure,
        CancellationToken ct = default) =>
        await (await result.ConfigureAwait(false)).MatchAsync(onSuccess, onFailure, ct)
            .ConfigureAwait(false);
    public static async ValueTask<TResult> MatchAsync<TResult>(this ValueTask<Result> result,
        Func<ValueTask<TResult>> onSuccess,
        Func<string, ValueTask<TResult>> onFailure,
        CancellationToken ct = default) =>
        await (await result.ConfigureAwait(false)).MatchAsync(onSuccess, onFailure, ct)
            .ConfigureAwait(false);

    // ----------------

    public static async ValueTask<Result<T>> OnError<T>(this ValueTask<Result<T>> result,
        Action<string> action) =>
        (await result.ConfigureAwait(false)).OnError(action);
    public static async ValueTask<Result<T>> FixError<T>(this ValueTask<Result<T>> result,
        Func<string, Result<T>> action) =>
        (await result.ConfigureAwait(false)).FixError(action);

    public static async ValueTask Then<T>(this ValueTask<Result<T>> result,
        Action<T> action) =>
        (await result.ConfigureAwait(false)).Then(action);
    public static async ValueTask<Result> Then<T>(this ValueTask<Result<T>> result,
        Func<T, Result> action) =>
        (await result.ConfigureAwait(false)).Then(action);
    public static async ValueTask<Result<TResult>> Then<T, TResult>(this ValueTask<Result<T>> result,
        Func<T, Result<TResult>> action) =>
        (await result.ConfigureAwait(false)).Then(action);

    public static async ValueTask ThenAsync<T>(this ValueTask<Result<T>> result,
        Func<T, CancellationToken, ValueTask> action,
        CancellationToken ct = default) =>
        await (await result.ConfigureAwait(false)).ThenAsync(action, ct).ConfigureAwait(false);
    public static async ValueTask<Result> ThenAsync<T>(this ValueTask<Result<T>> result,
        Func<T, CancellationToken, ValueTask<Result>> action,
        CancellationToken ct = default) =>
        await (await result.ConfigureAwait(false)).ThenAsync(action, ct).ConfigureAwait(false);
    public static async ValueTask<Result<TResult>> ThenAsync<T, TResult>(this ValueTask<Result<T>> result,
        Func<T, CancellationToken, ValueTask<Result<TResult>>> action,
        CancellationToken ct = default) =>
        await (await result.ConfigureAwait(false)).ThenAsync(action, ct).ConfigureAwait(false);

    public static async ValueTask<Result<TResult>> Map<T, TResult>(this ValueTask<Result<T>> result,
        Func<T, TResult> action) =>
        (await result.ConfigureAwait(false)).Map(action);
    public static async ValueTask<Result<TResult>> MapAsync<T, TResult>(this ValueTask<Result<T>> result,
        Func<T, CancellationToken, ValueTask<TResult>> action,
        CancellationToken ct = default) =>
        await (await result.ConfigureAwait(false)).MapAsync(action, ct).ConfigureAwait(false);

    public static async ValueTask Match<T>(this ValueTask<Result<T>> result,
        Action<T> onSuccess, Action<string> onFailure) =>
        (await result.ConfigureAwait(false)).Match(onSuccess, onFailure);
    public static async ValueTask<TResult> Match<T, TResult>(this ValueTask<Result<T>> result,
        Func<T, TResult> onSuccess, Func<string, TResult> onFailure) =>
        (await result.ConfigureAwait(false)).Match(onSuccess, onFailure);

    public static async ValueTask MatchAsync<T>(this ValueTask<Result<T>> result,
        Func<T, CancellationToken, ValueTask> onSuccess,
        Func<string, CancellationToken, ValueTask> onFailure,
        CancellationToken ct = default) =>
        await (await result.ConfigureAwait(false)).MatchAsync(onSuccess, onFailure, ct)
            .ConfigureAwait(false);
    public static async ValueTask<TResult> MatchAsync<T, TResult>(this ValueTask<Result<T>> result,
        Func<T, CancellationToken, ValueTask<TResult>> onSuccess,
        Func<string, CancellationToken, ValueTask<TResult>> onFailure,
        CancellationToken ct = default) =>
        await (await result.ConfigureAwait(false)).MatchAsync(onSuccess, onFailure, ct)
            .ConfigureAwait(false);
}
