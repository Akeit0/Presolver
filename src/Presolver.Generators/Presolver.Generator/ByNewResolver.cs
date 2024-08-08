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

    public ByNewResolver(string implementedPlace,INamedTypeSymbol type, ImmutableArray<ITypeSymbol> interfaces, Scope scope, PresolverContext refs) : base(implementedPlace,interfaces, scope)
    {
        Type = type;
        if (!refs.TypeDependencies.TryGetValue(type, out dependencies))
        {
            var applicableConstructors =
                (type.IsUnboundGenericType ? type.OriginalDefinition : type)
                .InstanceConstructors
                .Where(x => x.DeclaredAccessibility == Accessibility.Public).ToList();
            if(applicableConstructors.Count == 0)
            {
                dependencies = ImmutableArray<ITypeSymbol>.Empty;
                refs.TypeDependencies.Add(type, dependencies);
                return;
            }
            var ctor = applicableConstructors[0];
            var parameters = ctor.Parameters;
            if (parameters.Length == 0)
            {
                dependencies = ImmutableArray<ITypeSymbol>.Empty;
                if (!type.IsValueType) return;
                if (0 >= applicableConstructors.Count)
                {
                    dependencies = ImmutableArray<ITypeSymbol>.Empty;
                    refs.TypeDependencies.Add(type, dependencies);
                    return;
                }
                parameters = applicableConstructors[1].Parameters;
            }

            var builder = ImmutableArray.CreateBuilder<ITypeSymbol>(parameters.Length);

            foreach (var param in parameters)
            {
                param.ThrowIfNotResolvable();
                builder.Add(param.Type);
            }

            dependencies = builder.ToImmutable();
            refs.TypeDependencies.Add(type, dependencies);
        }
    }

    public override ITypeSymbol Type { get; }

    public override ImmutableArray<ITypeSymbol> TypeDependencies => dependencies;

    public override void WriteCode(CodeWriter writer,string? callerTypeName = null,string? interfaceName = null)
    {
        if (interfaceName==null)
        {
            writer.Append("new ");
            writer.Append(Type.ToFullyQualifiedString());
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
        builder.Append("[new] ");
        builder.Append(Scope.ToScopeString());
        builder.Append(' ');
        builder.Append(ImplementedPlace);
        builder.Append(' ');
        builder.Append(Type.ToFullyQualifiedString().Replace("global::", ""));
    }
}