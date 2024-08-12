#region

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

#endregion

namespace Presolver.Generator;

public sealed class ContainerSelfResolver(int id,string implementedPlace,ITypeSymbol containerBaseType) : Resolver(id,implementedPlace,ImmutableArray.Create(containerBaseType), Scope.Singleton)
{
    public override ITypeSymbol Type { get; } = containerBaseType;


    public override void WriteCode(CodeWriter writer, string? callerTypeName = null, string? interfaceName = null)
    {
        writer.Append(callerTypeName==null?"container":"c");
    }

    public override void WriteDebugInfo(StringBuilder builder)
    {
        builder.Append("[Self] ");
        builder.Append(ImplementedPlace);
    }
}