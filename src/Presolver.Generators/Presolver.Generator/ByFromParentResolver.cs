#region

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

#endregion

namespace Presolver.Generator;

public sealed class ByFromParentResolver : Resolver
{
    readonly Resolver parent;

    public ByFromParentResolver(Resolver parent, int depth) : base(parent.Interfaces, parent.Scope)
    {
        this.parent = parent;
        Depth = depth;
        Type = parent.Type;
        UsableTypeName = parent.UsableTypeName;
        Id = parent.Id;
    }

    public override ITypeSymbol Type { get; }

    public int Depth { get; }

    public override ImmutableArray<ITypeSymbol> TypeDependencies => parent.TypeDependencies;


    protected override Resolver[] Resolve(Dictionary<ITypeSymbol, Resolver> dictionary, ContainerTypeData? parentReference)
    {
        if (parent.Scope == Scope.Singleton) return [];

        return base.Resolve(dictionary, parentReference);
    }

    public override string WriteCode(CodeWriter writer, bool fromInternal)
    {
        UsableTypeName = parent.UsableTypeName;
        if (Scope != Scope.Scoped)
        {
            if (fromInternal) writer.Append("c.");
            for (var d = 0; d < Depth; d++) writer.Append("Parent.");

            writer.Append("__internalContainer.Resolve_");
            writer.Append(UsableTypeName);
            writer.Append("(");
        }
        else
        {
            if (fromInternal) writer.Append("c.");
            writer.Append("__internalScoped.");
            for (var d = 0; d < Depth; d++) writer.Append("Parent.");
            writer.Append("Resolve_");
            writer.Append(UsableTypeName);
            writer.Append("(");
        }

        return ")";
    }

    public override void WriteDebugInfo(StringBuilder builder, int index)
    {
        for (var d = 0; d < Depth; d++) builder.Append("Parent.");
        builder.Append(" ");
        parent.WriteDebugInfo(builder, index);
    }
}