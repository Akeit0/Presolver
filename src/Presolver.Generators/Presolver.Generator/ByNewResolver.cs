#region

using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

#endregion

namespace Presolver.Generator;

public sealed class ByNewResolver : Resolver
{
    readonly ImmutableArray<ITypeSymbol> dependencies;

    public ByNewResolver(INamedTypeSymbol type, ImmutableArray<ITypeSymbol> interfaces, Scope scope, PresolverContext refs) : base(interfaces, scope)
    {
        Type = type;
        if (!refs.TypeDependencies.TryGetValue(type, out dependencies))
        {
            var applicableConstructors =
                (type.IsUnboundGenericType ? type.OriginalDefinition : type)
                .InstanceConstructors
                .Where(x => x.DeclaredAccessibility == Accessibility.Public).ToList();
            var ctor = applicableConstructors[0];
            var parameters = ctor.Parameters;
            if (parameters.Length == 0)
            {
                dependencies = ImmutableArray<ITypeSymbol>.Empty;
                if (!type.IsValueType) return;
                if (0 >= applicableConstructors.Count) return;
                parameters = applicableConstructors[1].Parameters;
            }

            var builder = ImmutableArray.CreateBuilder<ITypeSymbol>(parameters.Length);

            foreach (var param in parameters) builder.Add(param.Type);

            dependencies = builder.ToImmutable();
            refs.TypeDependencies.Add(type, dependencies);
        }
    }

    public override ITypeSymbol Type { get; }

    public override ImmutableArray<ITypeSymbol> TypeDependencies => dependencies;

    public override string WriteCode(CodeWriter writer, bool fromInternal)
    {
        writer.Append($"new {Type.ToFullyQualifiedString()}(");
        return ")";
    }

    public override void WriteDebugInfo(StringBuilder builder, int index)
    {
        builder.Append("[new]");
        builder.Append(Type.ToFullyQualifiedString());
        builder.Append(" Parameter[");
        builder.Append(index);
        builder.Append("]");
    }
}