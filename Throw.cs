using System.Diagnostics;

namespace Keystone;

public static class Throw
{
    public static void Unreachable() => throw new UnreachableException();
    public static T Unreachable<T>() => throw new UnreachableException();

    public static void InvalidOperation(string message) => throw new InvalidOperationException(message);
    public static T InvalidOperation<T>(string message) => throw new InvalidOperationException(message);

    public static void InvalidArgument(string message) => throw new ArgumentException(message);
    public static T InvalidArgument<T>(string message) => throw new ArgumentException(message);
}

