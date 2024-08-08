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

    public ByFromParentResolver(Resolver parent, int depth) : base(parent.ImplementedPlace,parent.Interfaces, parent.Scope)
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

    public override void WriteCode(CodeWriter writer, string? callerTypeName, string? interfaceName)
    {
        if (interfaceName == null)
        {
            if (Scope != Scope.Scoped)
            {
                writer.Append("c.");
                for (var d = 0; d < Depth; d++) writer.Append("Parent.");

                writer.Append("__internalContainer.Resolve_");
                writer.Append(UsableTypeName);
                
            }
            else
            {
                 writer.Append("c.");
                writer.Append("__internalScoped.");
                for (var d = 0; d < Depth; d++) writer.Append("Parent.");
                writer.Append("Resolve_");
                writer.Append(UsableTypeName);
            }

            writer.Append("(");
            WriteDependencies(writer, callerTypeName);
            writer.Append(")");
        }
        else
        {
            WriteCodeDefault(writer, callerTypeName, interfaceName);
        }
    }

    public override void WriteDebugInfo(StringBuilder builder)
    {
        //for (var d = 0; d < Depth; d++) builder.Append("Parent.");
        //builder.Append(" ");
        parent.WriteDebugInfo(builder);
    }
}