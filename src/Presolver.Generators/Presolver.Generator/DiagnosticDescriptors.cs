#region

using Microsoft.CodeAnalysis;

#endregion

namespace Presolver.Generator;

internal static class DiagnosticDescriptors
{
    const string Category = "Presolver.Generator.Roslyn3";

    public static readonly DiagnosticDescriptor UnexpectedErrorDescriptor = new(
        "PRESOLVER001",
        "Unexpected error during generation",
        "Unexpected error occurred during code generation: {0}",
        "Usage",
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor MustBePartial = new(
        "PRESOLVER002",
        "Container type declaration must be partial",
        "Container type declaration '{0}' must be partial",
        Category,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor NestedNotAllow = new(
        "PRESOLVER003",
        "Container type must not be nested type",
        "Container '{0}' must be not nested type",
        Category,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor MustDeriveFromContainerBase = new(
        "PRESOLVER004",
        "Container type must derive from ContainerBase",
        "Container type '{0}' must derive from ContainerBase",
        Category,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor CantGeneratorMultipleTimes = new(
        id: "PRESOLVER005",
        "GenerateContainerAttribute cannot be attached more than once, including the inheritance source",
        "Container type '{0}' have already attached GenerateContainerAttribute in base class",
        Category,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor CircularDependency = new(
        "PRESOLVER006",
        "Circular dependency detected",
        "Circular dependency detected: {0}",
        Category,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor UnregisteredType = new(
        "PRESOLVER007",
        "Unregistered type",
        "Unregistered type: {0}",
        Category,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor UnresolvableParameter = new(
        "PRESOLVER008",
        "The parameter is not supported",
        "Parameter {0} is not supported",
        Category,
        DiagnosticSeverity.Error, true);
    
    public static readonly DiagnosticDescriptor NonPublicMethodInjections = new(
        "PRESOLVER009",
        "Non public method injection is not supported",
        "Non public injection {0} is not supported",
        Category,
        DiagnosticSeverity.Error, true);
    
}