#region

using System;
using Microsoft.CodeAnalysis;

#endregion

namespace Presolver.Generator;

public static class TypeExtensions
{
    public static string ToFullyQualifiedString(this ISymbol type)
    {
        return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }


    public static string UsableName(this ITypeSymbol type)
    {
        var name = type.ToFullyQualifiedString();
        if (!name.StartsWith("global::")) return name;
        Span<char> span = stackalloc char[name.Length - 8];
        for (var i = 8; i < name.Length; i++)
        {
            var c = name[i];
            span[i - 8] = c is '.' or '<' or '>' ? '_' : c;
        }

        return span.ToString();
    }
}