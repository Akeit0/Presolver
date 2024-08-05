#region

using System;
using Microsoft.CodeAnalysis;

#endregion

namespace Presolver.Generator;

public enum Scope
{
    Singleton,
    Transient,
    Scoped
}

public static class ScopeEx
{
    public static bool TryGetScope(this string name, out Scope value)
    {
        value = default;
        if (name.Length < 5) return false;
        switch (name.AsSpan().Slice(0, name.Length - 1))
        {
            case "Singleton`":
                value = Scope.Singleton;
                return true;
            case "Transient`":
                value = Scope.Transient;
                return true;
            case "Scoped`":
                value = Scope.Scoped;
                return true;
            default: return false;
        }
    }

    public static Scope ToScope(this ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is null) return Scope.Transient;
        return typeSymbol.Name switch
        {
            "Singleton" => Scope.Singleton,
            "Transient" => Scope.Transient,
            "Scoped" => Scope.Scoped,
            _ => Scope.Transient
        };
    }
}