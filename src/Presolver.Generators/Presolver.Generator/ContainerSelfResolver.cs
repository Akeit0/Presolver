#region

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

#endregion

namespace Presolver.Generator;

public sealed class ContainerSelfResolver(ITypeSymbol containerBaseType) : Resolver(ImmutableArray.Create(containerBaseType), Scope.Singleton)
{
    public override ITypeSymbol Type { get; } = containerBaseType;

    public override string WriteCode(CodeWriter writer, bool fromInternal)
    {
        writer.Append(fromInternal ? "c" : "this");
        return "";
    }

    public override void WriteDebugInfo(StringBuilder builder, int index)
    {
    }
}