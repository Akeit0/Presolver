#region

using System.Runtime.CompilerServices;

#endregion

namespace Presolver;

public static class ManualLazy
{
    public static ManualLazy<T> Create<T>()
    {
        return new(new());
    }

    public static void Dispose<T>(ref this ManualLazy<T> lazy) where T : IDisposable
    {
        lock (lazy.LockObject)
        {
            if (lazy.TryGetValue(out var value)) value.Dispose();
        }
    }
}

public struct ManualLazy<T>(object lockObject)
{
    public static ManualLazy<T> Create()
    {
        return new(new());
    }

    bool isValueCreated;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(out T value)
    {
        if (isValueCreated)
        {
            value = Value;
            return true;
        }

        value = default!;
        return false;
    }

    T value;

    public readonly object LockObject = lockObject;

    public T Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            isValueCreated = true;
            this.value = value;
        }
    }
}