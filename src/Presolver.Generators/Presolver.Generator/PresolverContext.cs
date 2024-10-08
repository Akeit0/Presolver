﻿#region

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#endregion

namespace Presolver.Generator;

public class PresolverContext(Compilation compilation, SourceProductionContext context)
{
    public INamedTypeSymbol IEnumerableType { get; } = compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1")!;

    public INamedTypeSymbol IReadOnlyListType { get; } = compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyList`1")!;
    public INamedTypeSymbol IDisposableType { get; } = compilation.GetTypeByMetadataName("System.IDisposable")!;
    public INamedTypeSymbol ContainerBaseType { get; } = compilation.GetTypeByMetadataName("Presolver.ContainerBase")!;
    
    public INamedTypeSymbol GenerateContainerAttributeType { get; } = compilation.GetTypeByMetadataName("Presolver.GenerateContainerAttribute")!;

    public bool IsValid => ContainerBaseType != null!;

    public Dictionary<ITypeSymbol, ImmutableArray<ITypeSymbol>> TypeDependencies { get; } = new(SymbolEqualityComparer.Default);

    Dictionary<ITypeSymbol, ModuleData> Modules { get; } = new(SymbolEqualityComparer.Default);

    public Dictionary<ITypeSymbol, (string? MethodName, ImmutableArray<ITypeSymbol> Dependency)> InjectMethods { get; } = new(SymbolEqualityComparer.Default);

    public Location? CurrentLocation { get; set; }

    public List<(Resolver, int)> DependencyStack { get; } = new();
    
    public bool CheckContainerIsValid(ITypeSymbol type, TypeDeclarationSyntax typeDeclaration)
    {
        if (!typeDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
        {
            ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.MustBePartial,
                typeDeclaration.Identifier.GetLocation(),
                type.Name));
            return false;
        }

        if (typeDeclaration.Parent is TypeDeclarationSyntax)
        {
            ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.NestedNotAllow,
                typeDeclaration.Identifier.GetLocation(),
                type.Name));
            return false;
        }

        var baseType = type.BaseType;
        var isFirst = true;
        while (baseType != null)
        {
            if(baseType.ContainingNamespace.Name == "Presolver"&& baseType.Name == "ContainerBase")
                return true;
            
            var attributes = baseType.GetAttributes();
            if (attributes.Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, GenerateContainerAttributeType)))
            {
                ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.CantGeneratorMultipleTimes, CurrentLocation, type.ToFullyQualifiedString()));
                return false;
            }

            baseType = baseType.BaseType;
            if(!isFirst&&baseType == null)
            {
                ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MustDeriveFromContainerBase, CurrentLocation, type.ToFullyQualifiedString()));
                return false;
            }
            isFirst = false;
        }

        return true;
    }

    public bool IsDisposable(ITypeSymbol type)
    {
        foreach (var i in type.OriginalDefinition.AllInterfaces)
            if (SymbolEqualityComparer.Default.Equals(i, IDisposableType))
                return true;
        return false;
    }

    public ModuleData GetOrAddModuleData(ITypeSymbol type)
    {
        if (Modules.TryGetValue(type, out var module)) return module;
        module = new(type);
        Modules[type] = module;
        return module;
    }


    public bool TryGetInjectMethod(ITypeSymbol type, out string? methodName, out ImmutableArray<ITypeSymbol> dependencies)
    {
        if (InjectMethods.TryGetValue(type, out var value))
        {
            methodName = value.MethodName;
            dependencies = value.Dependency;
            if (methodName == null) return false;
            return true;
        }

        foreach (var member in type.GetMembers())
            if (member is IMethodSymbol methodSymbol)
            {
                if (methodSymbol.DeclaredAccessibility != Accessibility.Public) continue;
                if (methodSymbol.GetAttributes().Any(x => x.AttributeClass?.Name == "InjectAttribute"))
                {
                    methodName = methodSymbol.Name;
                    var parameters = methodSymbol.Parameters;
                    var builder = ImmutableArray.CreateBuilder<ITypeSymbol>(parameters.Length);

                    for (var index = 0; index < parameters.Length; index++)
                    {
                        var param = parameters[index];
                        var paramType = param.Type;
                        builder.Add(paramType);
                    }

                    dependencies = builder.ToImmutable();
                    InjectMethods[type] = (methodName, dependencies);

                    return true;
                }
            }

        methodName = null;
        dependencies = ImmutableArray<ITypeSymbol>.Empty;
        InjectMethods[type] = (methodName, dependencies);
        return false;
    }


    public void ReportDiagnostic(Diagnostic diagnostic)
    {
        context.ReportDiagnostic(diagnostic);
    }

    public void ReportDiagnostic(PresolverGeneratorException exception)
    {
        if (exception is CircularDependencyException)
        {
            ReportCircularDependency(exception.Message);
        }
        else if (exception is UnregisteredTypeException)
        {
            ReportUnregisteredType(exception.Message);
        }
        else if(exception is UnresolvableParameterException unresolvableParameterException)
        {
            ReportUnresolvableParameter(unresolvableParameterException);
        }
    }
    

    public void ReportCircularDependency(string message)
    {
        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.CircularDependency, CurrentLocation, message));
    }

    public void ReportUnregisteredType(string message)
    {
        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UnregisteredType, CurrentLocation, message));
    }
    public void ReportUnresolvableParameter(UnresolvableParameterException exception)
    {
        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UnresolvableParameter, exception.Location??CurrentLocation, exception.Message));
    }
}