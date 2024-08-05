namespace Presolver;

public readonly struct Singleton<T>(T value)
{
    public readonly T Value = value;

    public static implicit operator Singleton<T>(T value)
    {
        return new(value);
    }
}

public readonly struct Singleton<TInterface, T>(T value) where T : TInterface
{
    public readonly T Value = value;

    public static implicit operator Singleton<TInterface, T>(T value)
    {
        return new(value);
    }
}

public readonly struct Singleton<TInterface0, TInterface1, T>(T value) where T : TInterface0, TInterface1
{
    public readonly T Value = value;

    public static implicit operator Singleton<TInterface0, TInterface1, T>(T value)
    {
        return new(value);
    }
}

public readonly struct Singleton<TInterface0, TInterface1, TInterface2, T>(T value) where T : TInterface0, TInterface1, TInterface2
{
    public readonly T Value = value;

    public static implicit operator Singleton<TInterface0, TInterface1, TInterface2, T>(T value)
    {
        return new(value);
    }
}

public readonly struct Singleton<TInterface0, TInterface1, TInterface2, TInterface3, T>(T value) where T : TInterface0, TInterface1, TInterface2, TInterface3
{
    public readonly T Value = value;

    public static implicit operator Singleton<TInterface0, TInterface1, TInterface2, TInterface3, T>(T value)
    {
        return new(value);
    }
}

public readonly struct Singleton<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, T>(T value) where T : TInterface0, TInterface1, TInterface2, TInterface3, TInterface4
{
    public readonly T Value = value;

    public static implicit operator Singleton<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, T>(T value)
    {
        return new(value);
    }
}

public readonly struct Singleton<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, T>(T value) where T : TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, TInterface5
{
    public readonly T Value = value;

    public static implicit operator Singleton<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, T>(T value)
    {
        return new(value);
    }
}