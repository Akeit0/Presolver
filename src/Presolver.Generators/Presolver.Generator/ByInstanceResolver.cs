﻿#region

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

#endregion

namespace Presolver.Generator;

public class ByInstanceResolver : Resolver
{
    public ByInstanceResolver(ITypeSymbol type, string name, ImmutableArray<ITypeSymbol> interfaces, InstanceOptions options, PresolverContext refs) : base(interfaces, Scope.Singleton)
    {
        Type = type;
        Name = name;
        Options = options;
        if ((options & InstanceOptions.Inject) != 0)
            if (refs.TryGetInjectMethod(type, out var method, out var types))
            {
                dependencies = types;
                methodName = method!;
                return;
            }

        dependencies = ImmutableArray<ITypeSymbol>.Empty;
    }

    string? methodName { get; }

    public string Name { get; }
    public InstanceOptions Options { get; }

    public override ITypeSymbol Type { get; }

    ImmutableArray<ITypeSymbol> dependencies { get; }

    public override ImmutableArray<ITypeSymbol> TypeDependencies => dependencies;


    public override string WriteCode(CodeWriter writer, bool fromInternal)
    {
        if (methodName is null) return "";
        writer.Append("v.");
        writer.Append(methodName);
        writer.Append("(");
        return ")";
    }

    public override void WriteDebugInfo(StringBuilder builder, int index)
    {
        builder.Append("[Instance] ");
        if (Name.EndsWith(".Value"))
            builder.Append(Name, 0, Name.Length - 6);
        else
            builder.Append(Name);

        if (methodName != null)
        {
            builder.Append(".").Append(methodName);
            builder.Append(" Parameter[");
            builder.Append(index);
            builder.Append("]");
        }
    }
}