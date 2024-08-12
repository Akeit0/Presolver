#region

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

#endregion

namespace Presolver.Generator;

public sealed class ByFactoryResolver : Resolver
{
    readonly string accessor;

    public ByFactoryResolver(int id,string implementedPlace,ITypeSymbol returnType, string accessor, IMethodSymbol method, ImmutableArray<ITypeSymbol> interfaces, Scope scope) : base(id,implementedPlace,interfaces, scope)
    {
        this.accessor = accessor;
        this.method = method;
        methodName = method.Name;
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
            param.ThrowIfNotResolvable();
            var paramType = param.Type;
            builder.Add(paramType);
        }

        dependencies = builder.ToImmutable();
    }

    IMethodSymbol method { get; }
    
    string methodName { get; }

    public override ITypeSymbol Type { get; }

    ImmutableArray<ITypeSymbol> dependencies { get; }

    public override ImmutableArray<ITypeSymbol> TypeDependencies => dependencies;

    public override void WriteCode(CodeWriter writer,string? callerTypeName = null,string? interfaceName = null)
    {
        if (interfaceName==null)
        {
            writer.Append("c.");
            writer.Append(accessor);
            writer.Append(methodName);
            writer.Append("(");
            WriteDependencies(writer, callerTypeName);
            writer.Append(").Value");
        }
        else
        {
            WriteCodeDefault(writer, callerTypeName, interfaceName);
        }
    }

    public override void WriteDebugInfo(StringBuilder builder)
    {
        builder.Append("[Factory] ");
        builder.Append(Scope.ToScopeString());
        builder.Append("  ");
        builder.Append(Type.ToFullyQualifiedString().Replace("global::", ""));
        builder.Append("  ");
        
        builder.Append(ImplementedPlace);
        builder.Append(' ');
        
        builder.Append(accessor);
        builder.Append(methodName);
      
    }
}