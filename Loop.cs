namespace Keystone;

public static class Loop
{
    public static void For(int size, Action<int> action)
    {
        for (int i = 0; i < size; i++)
            action?.Invoke(i);
    }

    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (T item in enumerable) action?.Invoke(item);
        return enumerable;
    }
}
