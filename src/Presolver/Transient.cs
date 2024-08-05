namespace Presolver;

public readonly struct Transient<T>(T value)
{
    public readonly T Value = value;

    public static implicit operator Transient<T>(T value)
    {
        return new(value);
    }
}

public readonly struct Transient<TInterface, T>(T value) where T : TInterface
{
    public readonly T Value = value;

    public static implicit operator Transient<TInterface, T>(T value)
    {
        return new(value);
    }
}

public readonly struct Transient<TInterface0, TInterface1, T>(T value) where T : TInterface0, TInterface1
{
    public readonly T Value = value;

    public static implicit operator Transient<TInterface0, TInterface1, T>(T value)
    {
        return new(value);
    }
}

public readonly struct Transient<TInterface0, TInterface1, TInterface2, T>(T value) where T : TInterface0, TInterface1, TInterface2
{
    public readonly T Value = value;

    public static implicit operator Transient<TInterface0, TInterface1, TInterface2, T>(T value)
    {
        return new(value);
    }
}

public readonly struct Transient<TInterface0, TInterface1, TInterface2, TInterface3, T>(T value) where T : TInterface0, TInterface1, TInterface2, TInterface3
{
    public readonly T Value = value;

    public static implicit operator Transient<TInterface0, TInterface1, TInterface2, TInterface3, T>(T value)
    {
        return new(value);
    }
}

public readonly struct Transient<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, T>(T value) where T : TInterface0, TInterface1, TInterface2, TInterface3, TInterface4
{
    public readonly T Value = value;

    public static implicit operator Transient<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, T>(T value)
    {
        return new(value);
    }
}

public readonly struct Transient<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, T>(T value) where T : TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, TInterface5
{
    public readonly T Value = value;

    public static implicit operator Transient<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, T>(T value)
    {
        return new(value);
    }
}