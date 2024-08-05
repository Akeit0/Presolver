namespace Presolver;

public readonly struct Service<T, TScope>(T value) where TScope : IScope
{
    public readonly T Value = value;

    public static implicit operator Service<T, TScope>(T value)
    {
        return new(value);
    }
}

public readonly struct Service<TInterface, T, TScope>(T value) where T : TInterface where TScope : IScope
{
    public readonly T Value = value;

    public static implicit operator Service<TInterface, T, TScope>(T value)
    {
        return new(value);
    }
}

public readonly struct Service<TInterface0, TInterface1, T, TScope>(T value) where T : TInterface0, TInterface1 where TScope : IScope
{
    public readonly T Value = value;

    public static implicit operator Service<TInterface0, TInterface1, T, TScope>(T value)
    {
        return new(value);
    }
}

public readonly struct Service<TInterface0, TInterface1, TInterface2, T, TScope>(T value) where T : TInterface0, TInterface1, TInterface2 where TScope : IScope
{
    public readonly T Value = value;

    public static implicit operator Service<TInterface0, TInterface1, TInterface2, T, TScope>(T value)
    {
        return new(value);
    }
}

public readonly struct Service<TInterface0, TInterface1, TInterface2, TInterface3, T, TScope>(T value) where T : TInterface0, TInterface1, TInterface2, TInterface3 where TScope : IScope
{
    public readonly T Value = value;

    public static implicit operator Service<TInterface0, TInterface1, TInterface2, TInterface3, T, TScope>(T value)
    {
        return new(value);
    }
}

public readonly struct Service<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, T, TScope>(T value) where T : TInterface0, TInterface1, TInterface2, TInterface3, TInterface4 where TScope : IScope
{
    public readonly T Value = value;

    public static implicit operator Service<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, T, TScope>(T value)
    {
        return new(value);
    }
}

public readonly struct Service<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, T, TScope>(T value) where T : TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, TInterface5 where TScope : IScope
{
    public readonly T Value = value;

    public static implicit operator Service<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, T, TScope>(T value)
    {
        return new(value);
    }
}