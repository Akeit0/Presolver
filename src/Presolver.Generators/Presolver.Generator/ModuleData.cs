#region

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.DotnetRuntime.Extensions;

#endregion

namespace Presolver.Generator;

public sealed class ModuleData
{
    readonly List<FactoryData> factories = new();
    readonly List<InstanceData> instances = new();

    public ModuleData(ITypeSymbol moduleType)
    {
        var members = moduleType.GetMembers();
        foreach (var member in members)
        {
            if (member.DeclaredAccessibility is not Accessibility.Public and Accessibility.Internal) continue;
            if (member is IMethodSymbol method)
            {
                var attributes = method.GetAttributes();
                var factoryAttribute = attributes.FirstOrDefault(x => x.AttributeClass?.OriginalDefinition.MetadataName is "Factory" or "FactoryAttribute");
                if (factoryAttribute != null)
                {
                    var returnType = method.ReturnType;
                    if (returnType is INamedTypeSymbol namedTypeSymbol)
                        if (returnType.ContainingNamespace.Name == "Presolver")
                        {
                            var metadataName = returnType.OriginalDefinition.MetadataName;
                            if (metadataName.StartsWith("Service"))
                            {
                                var typeArguments = namedTypeSymbol.TypeArguments;
                                var registerInterfaces = typeArguments.AsSpan().Slice(0, Math.Max(1, typeArguments.Length - 3)).ToImmutableArray();
                                var scope = typeArguments[typeArguments.Length - 1].ToScope();


                                factories.Add(new((INamedTypeSymbol)typeArguments[typeArguments.Length - 2], "", method, registerInterfaces, typeArguments, scope));
                            }
                            else if (metadataName.TryGetScope(out var scope))
                            {
                                var typeArguments = namedTypeSymbol.TypeArguments;
                                var registerInterfaces = typeArguments.AsSpan().Slice(0, Math.Max(1, typeArguments.Length - 2)).ToImmutableArray();
                                factories.Add(new((INamedTypeSymbol)typeArguments[typeArguments.Length - 1], "", method, registerInterfaces, typeArguments, scope));
                            }
                        }
                }
            }

            if (member is IPropertySymbol property)
            {
                var attributes = property.GetAttributes();
                var factoryAttribute = attributes.FirstOrDefault(x => x.AttributeClass?.OriginalDefinition.MetadataName is "Instance" or "InstanceAttribute");
                if (factoryAttribute != null)
                {
                    var returnType = property.Type;
                    var options = (InstanceOptions)(int)factoryAttribute.ConstructorArguments[0].Value!;
                    if (returnType is INamedTypeSymbol namedTypeSymbol)
                    {
                        var metadataName = returnType.OriginalDefinition.MetadataName;
                        if (metadataName.StartsWith("Service"))
                        {
                            var typeArguments = namedTypeSymbol.TypeArguments;
                            var registerInterfaces = typeArguments.AsSpan().Slice(0, Math.Max(1, typeArguments.Length - 3)).ToImmutableArray();
                            var scope = typeArguments[typeArguments.Length - 1].ToScope();
                            if (scope != Scope.Singleton) continue;
                            instances.Add(new(typeArguments[typeArguments.Length - 2], property.Name + ".Value", registerInterfaces, options));


                            continue;
                        }
                        else if (metadataName.TryGetScope(out var scope))
                        {
                            if (scope != Scope.Singleton) continue;

                            var typeArguments = namedTypeSymbol.TypeArguments;
                            var registerInterfaces = typeArguments.AsSpan().Slice(0, Math.Max(1, typeArguments.Length - 2)).ToImmutableArray();
                            instances.Add(new(typeArguments[typeArguments.Length - 2], property.Name + ".Value", registerInterfaces, options));

                            continue;
                        }
                    }

                    {
                        instances.Add(new(returnType, property.Name, ImmutableArray.Create(returnType), options));
                    }
                }
            }
        }
    }


    public void AddResolvers(string propertyAccessor, Dictionary<ITypeSymbol, List<Resolver>> interfaceToResolver, PresolverContext refs)
    {
        void Add(ITypeSymbol type, Resolver resolver)
        {
            if (!interfaceToResolver.TryGetValue(type, out var list))
            {
                list = new();
                interfaceToResolver[type] = list;
            }

            list.Add(resolver);
        }

        foreach (var factoryData in factories)
        {
            var resolver = new ByFactoryResolver(factoryData.ReturnType, propertyAccessor + factoryData.Accessor, factoryData.Method, factoryData.Interfaces, factoryData.Scope);
            foreach (var interfaceType in factoryData.Interfaces) Add(interfaceType, resolver);
        }

        foreach (var instance in instances)
        {
            var resolver = new ByInstanceResolver(instance.Type, propertyAccessor + instance.Name, instance.Interfaces, instance.Options, refs);
            foreach (var interfaceType in instance.Interfaces) Add(interfaceType, resolver);
        }
    }


    record FactoryData(ITypeSymbol ReturnType, string Accessor, IMethodSymbol Method, ImmutableArray<ITypeSymbol> Interfaces, ImmutableArray<ITypeSymbol> Dependencies, Scope Scope);

    record InstanceData(ITypeSymbol Type, string Name, ImmutableArray<ITypeSymbol> Interfaces, InstanceOptions Options);
}