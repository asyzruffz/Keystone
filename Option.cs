namespace Keystone;

public abstract record Option<T>
{
    public static Option<T> Some(T data) => Some<T>.Of(data);
    public static Option<T> None() => None<T>.Existed();
    public static Option<T> From(T? data) => data is null ? None() : Some(data!);

    public abstract T Data { get; }
    public abstract T Or(T defaultVal);

    public abstract bool IsNone { get; }
    public bool IsSome => !IsNone;

    public abstract void DoWith(Action<T> action);
    public abstract ValueTask DoWithAsync(Func<T, CancellationToken, ValueTask> action, CancellationToken ct);

    public abstract Option<TResult> Map<TResult>(Func<T, TResult> map);
    public abstract ValueTask<Option<TResult>> MapAsync<TResult>(
        Func<T, CancellationToken, ValueTask<TResult>> map, CancellationToken ct);

    public abstract Option<TResult> Then<TResult>(Func<T, Option<TResult>> action);
    public abstract ValueTask<Option<TResult>> ThenAsync<TResult>(
        Func<T, CancellationToken, ValueTask<Option<TResult>>> action, CancellationToken ct);

    public void Match(Action<T> onSome, Action onNone)
    {
        switch (this)
        {
            case Some<T> some: onSome(some.Data); break;
            case None<T>: onNone(); break;
            default: break;
        }
    }
    public TResult Match<TResult>(Func<T, TResult> onSome, Func<TResult> onNone) => this switch
    {
        Some<T> some => onSome(some.Data),
        None<T> => onNone(),
        _ => Throw.Unreachable<TResult>(),
    };

    public ValueTask MatchAsync(
        Func<T, CancellationToken, ValueTask> onSome,
        Func<CancellationToken, ValueTask> onNone,
        CancellationToken ct) => this switch
        {
            Some<T> some => onSome(some.Data, ct),
            None<T> => onNone(ct),
            _ => Throw.Unreachable<ValueTask>(),
        };
    public ValueTask<TResult> MatchAsync<TResult>(
        Func<T, CancellationToken, ValueTask<TResult>> onSome,
        Func<CancellationToken, ValueTask<TResult>> onNone,
        CancellationToken ct) => this switch
        {
            Some<T> some => onSome(some.Data, ct),
            None<T> => onNone(ct),
            _ => Throw.Unreachable<ValueTask<TResult>>(),
        };
}

public sealed record Some<T> : Option<T>, IEquatable<Some<T>>
{
    private T data;

    public static Option<T> Of(T data) => new Some<T>(data);
    private Some(T data) => this.data = data;

    public override T Data => data;
    public override T Or(T defaultVal) => data;

    public override bool IsNone => false;

    public override void DoWith(Action<T> action) => action(data);
    public override ValueTask DoWithAsync(Func<T, CancellationToken, ValueTask> action, CancellationToken ct) =>
        action(data, ct);

    public override Option<TResult> Map<TResult>(Func<T, TResult> map) => Option<TResult>.Some(map(data));
    public override async ValueTask<Option<TResult>> MapAsync<TResult>(
        Func<T, CancellationToken, ValueTask<TResult>> map, CancellationToken ct) =>
        Option<TResult>.Some(await map(data, ct).ConfigureAwait(false));

    public override Option<TResult> Then<TResult>(Func<T, Option<TResult>> action) => action(data);
    public override ValueTask<Option<TResult>> ThenAsync<TResult>(
        Func<T, CancellationToken, ValueTask<Option<TResult>>> action, CancellationToken ct) =>
        action(data, ct);

    public override string ToString() => $"Some({data})";
}

public sealed record None<T> : Option<T>, IEquatable<None<T>>
{
    static None<T> value = new None<T>();

    public static Option<T> Existed() => value;
    private None() { }

    public override T Data => Throw.InvalidOperation<T>("Invalid data in None");
    public override T Or(T defaultVal) => defaultVal;

    public override bool IsNone => true;

    public override void DoWith(Action<T> onSome) { }
    public override ValueTask DoWithAsync(Func<T, CancellationToken, ValueTask> onSome, CancellationToken ct) =>
        ValueTask.CompletedTask;

    public override Option<TResult> Map<TResult>(Func<T, TResult> map) => Option<TResult>.None();
    public override ValueTask<Option<TResult>> MapAsync<TResult>(
        Func<T, CancellationToken, ValueTask<TResult>> map, CancellationToken ct) =>
        ValueTask.FromResult(Option<TResult>.None());

    public override Option<TResult> Then<TResult>(Func<T, Option<TResult>> action) => Option<TResult>.None();
    public override ValueTask<Option<TResult>> ThenAsync<TResult>(
        Func<T, CancellationToken, ValueTask<Option<TResult>>> action, CancellationToken ct) =>
        ValueTask.FromResult(Option<TResult>.None());

    public override string ToString() => $"None<{typeof(T).Name}>";
}

public static class OptionExtensions
{
    public static Option<T> AsOption<T>(this T? data) => Option<T>.From(data);
    public static Result<T> ToResult<T>(this Option<T> option, string errorMessage = "") =>
        option.Match(Result<T>.Ok, () => Result<T>.Fail(errorMessage));

    // ----------------

    public static async ValueTask DoWith<T>(this ValueTask<Option<T>> option,
        Action<T> action) =>
        (await option.ConfigureAwait(false)).DoWith(action);
    public static async ValueTask DoWithAsync<T>(this ValueTask<Option<T>> option,
        Func<T, CancellationToken, ValueTask> action,
        CancellationToken ct) =>
        await (await option.ConfigureAwait(false)).DoWithAsync(action, ct).ConfigureAwait(false);

    public static async ValueTask<Option<TResult>> Map<T, TResult>(this ValueTask<Option<T>> option,
        Func<T, TResult> map) =>
        (await option.ConfigureAwait(false)).Map(map);
    public static async ValueTask<Option<TResult>> MapAsync<T, TResult>(this ValueTask<Option<T>> option,
        Func<T, CancellationToken, ValueTask<TResult>> map,
        CancellationToken ct) =>
        await (await option.ConfigureAwait(false)).MapAsync(map, ct).ConfigureAwait(false);

    public static async ValueTask<Option<TResult>> Then<T, TResult>(this ValueTask<Option<T>> option,
        Func<T, Option<TResult>> action) =>
        (await option.ConfigureAwait(false)).Then(action);
    public static async ValueTask<Option<TResult>> ThenAsync<T, TResult>(this ValueTask<Option<T>> option,
        Func<T, CancellationToken, ValueTask<Option<TResult>>> action,
        CancellationToken ct) =>
        await (await option.ConfigureAwait(false)).ThenAsync(action, ct).ConfigureAwait(false);

    public static async ValueTask Match<T>(this ValueTask<Option<T>> option,
        Action<T> onSome, Action onNone) =>
        (await option.ConfigureAwait(false)).Match(onSome, onNone);
    public static async ValueTask<TResult> Match<T, TResult>(this ValueTask<Option<T>> option,
        Func<T, TResult> onSome, Func<TResult> onNone) =>
        (await option.ConfigureAwait(false)).Match(onSome, onNone);

    public static async ValueTask MatchAsync<T>(this ValueTask<Option<T>> option,
        Func<T, CancellationToken, ValueTask> onSome,
        Func<CancellationToken, ValueTask> onNone,
        CancellationToken ct) =>
        await (await option.ConfigureAwait(false)).MatchAsync(onSome, onNone, ct)
            .ConfigureAwait(false);
    public static async ValueTask<TResult> MatchAsync<T, TResult>(this ValueTask<Option<T>> option,
        Func<T, CancellationToken, ValueTask<TResult>> onSome,
        Func<CancellationToken, ValueTask<TResult>> onNone,
        CancellationToken ct) =>
        await (await option.ConfigureAwait(false)).MatchAsync(onSome, onNone, ct)
            .ConfigureAwait(false);
}
