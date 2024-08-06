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


    public static string ToUsableName(this ITypeSymbol type)
    {
        var name = type.ToFullyQualifiedString().Replace("global::", "");
        Span<char> span = stackalloc char[name.Length];
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            
            span[i] = c is '.' or '<' or '>' ? '_' : c;
        }

        return span.ToString();
    }
    
    public static void ThrowIfNotResolvable(this IParameterSymbol param)
    {
        if (!param.IsResolvable())
        {
            throw new UnresolvableParameterException($"{param.ContainingSymbol.ToFullyQualifiedString()} {param.RefKind} {param.Type.ToFullyQualifiedString()} {param.Name}",param);
        }
    }
    public static bool IsResolvable(this IParameterSymbol param)
    {
        return (param.RefKind is  RefKind.None or RefKind.In)&&IsResolvableType(param.Type);
    }
    public static bool IsResolvableType(this ITypeSymbol type)
    {
        if (type.IsRefLikeType) return false;
        if(type.TypeKind==TypeKind.Pointer) return false;
        if(type.TypeKind==TypeKind.FunctionPointer) return false;
        return true;
    }
   
}