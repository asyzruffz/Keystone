namespace Keystone;

public abstract class Option<T>
{
    public static Option<T> Some(T data) => Some<T>.Of(data);
    public static Option<T> None() => None<T>.Existed();
    public static Option<T> From(T? data) => data is null ? None() : Some(data!);

    public abstract T Data { get; }
    public abstract T Or(T defaultVal);

    public abstract bool IsNone { get; }
    public bool IsSome => !IsNone;

    public abstract void DoWith(Action<T> onSome);
    public abstract Option<TResult> Map<TResult>(Func<T, TResult> map);
    public abstract Option<TResult> Then<TResult>(Func<T, Option<TResult>> action);
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

    #region Equality
    public static bool operator ==(Option<T>? a, Option<T>? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(Option<T>? a, Option<T>? b) => !(a == b);
    public override bool Equals(object? other) => other is Option<T> &&
        Match(onSome: _ => Equals(other as Some<T>), onNone: () => Equals(other as None<T>));
    public override int GetHashCode() => base.GetHashCode();
    #endregion
}

public class Some<T> : Option<T>, IEquatable<Some<T>>
{
    private T data;

    public static Option<T> Of(T data) => new Some<T>(data);
    private Some(T data) => this.data = data;

    public override T Data => data;
    public override T Or(T defaultVal) => data;

    public override bool IsNone => false;

    public override void DoWith(Action<T> onSome) => onSome(data);
    public override Option<TResult> Map<TResult>(Func<T, TResult> map) => Option<TResult>.Some(map(data));
    public override Option<TResult> Then<TResult>(Func<T, Option<TResult>> action) => action(data);
    public override string ToString() => $"Some({data})";

    #region Equality
    public static bool operator ==(Some<T>? a, Some<T>? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(Some<T>? a, Some<T>? b) => !(a == b);
    public override bool Equals(object? other) => other is Some<T> && Equals(other as Some<T>);
    public bool Equals(Some<T>? other) => other is not null && EqualityComparer<T>.Default.Equals(data, other.data);
    public override int GetHashCode() => data!.GetHashCode();
    #endregion
}

public class None<T> : Option<T>, IEquatable<None<T>>
{
    static None<T> value = new None<T>();

    public static Option<T> Existed() => value;
    private None() { }

    public override T Data => Throw.InvalidOperation<T>("Invalid data in None");
    public override T Or(T defaultVal) => defaultVal;

    public override bool IsNone => true;
    
    public override void DoWith(Action<T> onSome) { }
    public override Option<TResult> Map<TResult>(Func<T, TResult> map) => Option<TResult>.None();
    public override Option<TResult> Then<TResult>(Func<T, Option<TResult>> action) => Option<TResult>.None();
    public override string ToString() => $"None<{typeof(T).Name}>";

    #region Equality
    public static bool operator ==(None<T>? a, None<T>? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(None<T>? a, None<T>? b) => !(a == b);
    public override bool Equals(object? other) => other is None<T> && Equals(other as None<T>);
    public bool Equals(None<T>? other) => other is not null;
    public override int GetHashCode() => 0;
    #endregion
}

public static class OptionExtensions
{
    public static Option<T> AsOption<T>(this T? data) => Option<T>.From(data);
    public static Result<T> ToResult<T>(this Option<T> option, string errorMessage = "") => 
        option.Match(Result<T>.Ok, () => Result<T>.Fail(errorMessage));
}
