﻿#region

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

#endregion

namespace Presolver.Generator;

public sealed class ByFactoryResolver : Resolver
{
    readonly string accessor;

    public ByFactoryResolver(ITypeSymbol returnType, string accessor, IMethodSymbol method, ImmutableArray<ITypeSymbol> interfaces, Scope scope) : base(interfaces, scope)
    {
        this.accessor = accessor;
        this.method = method;
        Type = returnType;
        var parameters = method.Parameters;
        if (parameters.Length == 0)
        {
            dependencies = ImmutableArray<ITypeSymbol>.Empty;
            return;
        }

        var builder = ImmutableArray.CreateBuilder<ITypeSymbol>(parameters.Length);

        foreach (var param in parameters)
        {
            var paramType = param.Type;
            if (paramType is { } namedTypeSymbol) builder.Add(namedTypeSymbol);
        }

        dependencies = builder.ToImmutable();
    }

    IMethodSymbol method { get; }

    public override ITypeSymbol Type { get; }

    ImmutableArray<ITypeSymbol> dependencies { get; }

    public override ImmutableArray<ITypeSymbol> TypeDependencies => dependencies;


    public override string WriteCode(CodeWriter writer, bool fromInternal)
    {
        if (fromInternal) writer.Append("c.");
        writer.Append(accessor);
        writer.Append(method.ToFullyQualifiedString());
        writer.Append("(");
        return ").Value";
    }

    public override void WriteDebugInfo(StringBuilder builder, int index)
    {
        builder.Append("[Factory]");
        builder.Append(accessor);
        builder.Append(method.ToFullyQualifiedString());
        builder.Append(" Parameter[");
        builder.Append(index);
        builder.Append("]");
    }
}