#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.DotnetRuntime.Extensions;
using GeneratorAttributeSyntaxContext = Microsoft.CodeAnalysis.DotnetRuntime.Extensions.GeneratorAttributeSyntaxContext;

#endregion

namespace Presolver.Generator;

internal class Comparer : IEqualityComparer<(GeneratorAttributeSyntaxContext, Compilation)>
{
    public static readonly Comparer Instance = new();

    public bool Equals((GeneratorAttributeSyntaxContext, Compilation) x, (GeneratorAttributeSyntaxContext, Compilation) y)
    {
        return x.Item1.TargetNode.Equals(y.Item1.TargetNode);
    }

    public int GetHashCode((GeneratorAttributeSyntaxContext, Compilation) obj)
    {
        return obj.Item1.TargetNode.GetHashCode();
    }
}

internal static class Ex
{
    public static string NameWithGenerics(this ITypeSymbol type)
    {
        return type.ToDisplayString(new SymbolDisplayFormat(genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters));
    }

    public static string FullName(this INamespaceSymbol @namespace)
    {
        return @namespace.ToDisplayString(new(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces));
    }
}

[Generator]
internal class PresolverGenerator : IIncrementalGenerator
{
    [ThreadStatic]
    static StringBuilder? logBuilder;
    public static StringBuilder LogBuilder => logBuilder ??= new();


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                context,
                "Presolver.GenerateResolverAttribute",
                static (node, cancellation) => node is ClassDeclarationSyntax,
                static (context, cancellation) => context)
            .Combine(context.CompilationProvider)
            .WithComparer(Comparer.Instance);
        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(provider.Collect()),
            (sourceProductionContext, t) =>
            {
                var ct = sourceProductionContext.CancellationToken;
                var (compilation, list) = t;

                var generationContext = new PresolverContext(compilation, sourceProductionContext);
                if (!generationContext.IsValid) return;
                var containers = new Dictionary<ITypeSymbol, ContainerTypeData>(SymbolEqualityComparer.Default);
                var file = new CodeWriter(0);
                var scopedIndenter = new CodeWriter(0);
                var errorBuilder = LogBuilder;
                LogBuilder.Clear();
                errorBuilder.AppendLine("/* Error Log: ");
                errorBuilder.AppendLine(DateTime.Now.ToString(CultureInfo.InvariantCulture));
                var tick = Stopwatch.GetTimestamp();
                var timer = new Stopwatch();
                foreach (var (x, _) in list)
                {
                    if (ct.IsCancellationRequested) return;

                    var type = (ITypeSymbol)x.TargetSymbol;
                    var node = (TypeDeclarationSyntax)x.TargetNode;

                    generationContext.CurrentLocation = node.GetLocation();


                    if (!generationContext.CheckContainerIsValid(type, node)) continue;

                    errorBuilder.AppendLine(type.NameWithGenerics());
                    try
                    {
                        timer.Restart();

                        if (!containers.TryGetValue(type, out var graph)) graph = new(type, containers, generationContext);

                        if (ct.IsCancellationRequested) return;
                        var exceptions = graph.Exceptions;
                        if (exceptions.Count==0)
                        {
                            try
                            {
                                graph.Resolve();
                                graph.Write(file, scopedIndenter);
                            }
                            catch (Exception e)
                            {
                                exceptions.Add(e);
                            }
                        }

                        if (exceptions .Count!=0)
                        {
                            foreach (var exception in exceptions)
                            {
                                if(exception is PresolverGeneratorException presolverGeneratorException)
                                {
                                    generationContext.ReportDiagnostic(presolverGeneratorException);
                                }
                                else
                                {
                                    sourceProductionContext.ReportDiagnostic(
                                        Diagnostic.Create(
                                            DiagnosticDescriptors.UnexpectedErrorDescriptor,
                                            node.Identifier.GetLocation(), exception.Message
                                        )
                                    );
                                }
                                errorBuilder.AppendLine(exception.ToString());
                            }
                          
                            file.Clear();
                            scopedIndenter.Clear();
                           
                            try
                            {
                                graph.WriteFallBack(file, scopedIndenter);
                            }
                            catch (Exception fallbackException)
                            {
                                errorBuilder.AppendLine("Fallback" + fallbackException.Message);
                            }
                        }


                        var fullType = type.ToUsableName();

                        sourceProductionContext.AddSource($"{fullType}.g.cs", file.ToString());
                        errorBuilder.Append("End ");
                        errorBuilder.Append(timer.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
                        errorBuilder.AppendLine("ms");
                        timer.Reset();
                    }
                    catch (Exception e)
                    {
                        errorBuilder.AppendLine(e.Message);
                        sourceProductionContext.ReportDiagnostic(
                            Diagnostic.Create(
                                DiagnosticDescriptors.UnexpectedErrorDescriptor,
                                x.TargetNode.GetLocation(), e.Message
                            )
                        );
                    }

                    file.Clear();
                    scopedIndenter.Clear();
                }

                var totalTick = Stopwatch.GetTimestamp() - tick;
                errorBuilder.Append("Total ");
                errorBuilder.Append((totalTick * 1000.0 / Stopwatch.Frequency).ToString(CultureInfo.InvariantCulture));
                errorBuilder.Append(" ms");
                errorBuilder.AppendLine("*/");
                sourceProductionContext.AddSource("Log.txt", errorBuilder.ToString());
                errorBuilder.Clear();
            });
    }
}