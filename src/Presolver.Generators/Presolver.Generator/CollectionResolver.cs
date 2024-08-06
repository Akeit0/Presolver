#region

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

#endregion

namespace Presolver.Generator;

public class CollectionResolver(ITypeSymbol collectionType, ImmutableArray<ITypeSymbol> interfaces, List<Resolver> list, List<ByFromParentResolver> parentMetas) : Resolver(interfaces, Scope.Transient)
{
    public override ITypeSymbol Type => collectionType;

    public ITypeSymbol ElementType { get; } = ((INamedTypeSymbol)collectionType).TypeArguments[0];


    protected override Resolver[] Resolve(Dictionary<ITypeSymbol, Resolver> dictionary, ContainerTypeData? parentReference)
    {
        return list.Concat(parentMetas).ToArray();
    }

    public override void WriteCode(CodeWriter writer,string? callerTypeName,string? interfaceName)
    {
        if (interfaceName==null)
        {
            writer.Append("new ");
            writer.Append(ElementType.ToFullyQualifiedString());
            writer.Append("[]{");
            WriteDependencies(writer, callerTypeName);
            writer.Append("}");
        }
        else
        {
            var isSingleton = callerTypeName != null;
            writer.Append("global::Presolver.ResolveExtensions.ResolveAll<");
            writer.Append(isSingleton?callerTypeName!:"TContainer");
            writer.Append(",");
            writer.Append(ElementType.ToFullyQualifiedString());
            writer.Append(isSingleton?">(c)":">(container)");
        }
    }

    public override void WriteDebugInfo(StringBuilder builder, int index)
    {
        builder.Append("Collection  ");
        builder.Append(ElementType.ToFullyQualifiedString());
        builder.Append(" [");
        builder.Append(index);
        builder.Append("]");
    }
}