#region

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

#endregion

namespace Presolver.Generator;

public abstract class Resolver(string implementedPlace,ImmutableArray<ITypeSymbol> interfaces, Scope scope)
{
    
    
    string? usableTypeName;
    public ReadOnlySpan<Resolver> Dependencies => dependencies;
    Resolver[]? dependencies { get; set; }

    public bool IsResolved => dependencies != null;
    public Scope Scope { get; } = scope;


    public int? Id { get; set; }

    public string ImplementedPlace { get; }=implementedPlace;

    public ImmutableArray<ITypeSymbol> Interfaces { get; } = interfaces;


    public virtual ImmutableArray<ITypeSymbol> TypeDependencies { get; protected set; } = ImmutableArray<ITypeSymbol>.Empty;

    public abstract ITypeSymbol Type { get; }

    public string UsableTypeName
    {
        get
        {
            if (usableTypeName != null) return usableTypeName;
            if (Id.HasValue) return usableTypeName = Type.ToUsableName() + Id.Value;
            return usableTypeName = Type.ToUsableName();
        }
        protected set => usableTypeName = value;
    }


    public void ResolveSelf(Dictionary<ITypeSymbol, Resolver> dictionary, ContainerTypeData? parentReference)
    {
        if (dependencies != null) return;
        dependencies = Resolve(dictionary, parentReference);
    }


    protected virtual Resolver[] Resolve(Dictionary<ITypeSymbol, Resolver> dictionary, ContainerTypeData? parentReference)
    {
        var typeDependencies = TypeDependencies;
        var result = new Resolver[typeDependencies.Length];
        for (var index = 0; index < typeDependencies.Length; index++)
        {
            var dependency = typeDependencies[index];

            {
                var depth = 0;
                if (dictionary.TryGetValue(dependency, out var m))
                {
                    result[index] = m;
                    continue;
                }

                if (parentReference != null && parentReference.TryGetResolveMethod(dependency, out m, ref depth))
                {
                    m!.ResolveSelf(dictionary, parentReference);
                    result[index] = m!;
                    continue;
                }

                var builder = new StringBuilder();
                WriteDebugInfo(builder);
                builder.Append(" [");
                builder.Append(index);
                builder.Append("] -> ");
                builder.Append(dependency.ToFullyQualifiedString());
                throw new UnregisteredTypeException(builder.ToString());
            }
        }

        return result;
    }

    public abstract void WriteCode(CodeWriter writer,string? callerTypeName =null,string? interfaceName=null);

    protected static void WriteCodeDefault(CodeWriter writer,string? callerTypeName, string interfaceName)
    {
        var isSingleton = callerTypeName != null;
        writer.Append("global::Presolver.ResolveExtensions.Resolve<");
        writer.Append(isSingleton?callerTypeName!:"TContainer");
        writer.Append(",");
        writer.Append(interfaceName);
        writer.Append(isSingleton?">(c)":">(container)");
    }
    
    protected void WriteDependencies(CodeWriter writer,string? callerTypeName)
    {
        bool first = true;
        for (var index = 0; index < Dependencies.Length; index++)
        {
            if(first)
            {
                first = false;
            }else  writer.Append(", ");
            var resolver = Dependencies[index];
            resolver.WriteCode(writer, callerTypeName, TypeDependencies[index].ToFullyQualifiedString());
            
        }
    }


    public abstract void WriteDebugInfo(StringBuilder builder);
}