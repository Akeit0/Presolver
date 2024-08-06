using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Presolver.Generator;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class Analyzer : DiagnosticAnalyzer
{
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterCompilationStartAction(static context =>
        {
            if (context.Compilation.GetTypeByMetadataName("Presolver.ContainerBase")!=null)
            {
                context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ClassDeclaration,SyntaxKind.MethodDeclaration);
            }
        });
    }

    static void Analyze(SyntaxNodeAnalysisContext context)
    {
       
        if(context.Node is TypeDeclarationSyntax typeDeclaration)
        {
            var declaredSymbol = context.SemanticModel.GetDeclaredSymbol(typeDeclaration);

            if (declaredSymbol is null) return;
            var attributes = declaredSymbol.GetAttributes();
            if (attributes.Any(x => x.AttributeClass?.OriginalDefinition.MetadataName is "GenerateResolverAttribute" or "GenerateResolver"))
            {
                if (!typeDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.MustBePartial,
                        typeDeclaration.Identifier.GetLocation(),
                        declaredSymbol.Name));
                }

                if (typeDeclaration.Parent is TypeDeclarationSyntax)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.NestedNotAllow,
                        typeDeclaration.Identifier.GetLocation(),
                        declaredSymbol.Name));
                }
            }
        }
        else if(context.Node is MethodDeclarationSyntax methodDeclaration)
        {
            var declaredSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration);
            if (declaredSymbol is null) return;
            var attributes = declaredSymbol.GetAttributes();
            if (attributes.Any(x => x.AttributeClass?.OriginalDefinition.MetadataName is "Inject" or "InjectAttribute"))
            {
                string? methodName=null;
                var publicFlag = false;
                foreach (var token in methodDeclaration.Modifiers)
                {
                    if (token.IsKind(SyntaxKind.PublicKeyword))
                    {
                        publicFlag = true;
                        break;
                    }
                    if (token.IsKind(SyntaxKind.PrivateKeyword)||token.IsKind(SyntaxKind.ProtectedKeyword))
                    {
                        break;
                    }
                }
                if(!publicFlag)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.NonPublicMethodInjections,
                        methodDeclaration.Identifier.GetLocation(),
                        (methodName = declaredSymbol.ContainingType.ToFullyQualifiedString()+"."+declaredSymbol.ToFullyQualifiedString())));
                }
                foreach (var parameterSymbol in declaredSymbol.Parameters)
                {
                    if (!parameterSymbol.IsResolvable())
                    {
                        methodName ??= declaredSymbol.ContainingType.ToFullyQualifiedString()+"."+declaredSymbol.ToFullyQualifiedString();
                        context.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticDescriptors.UnresolvableParameter,
                            methodDeclaration.Identifier.GetLocation(),
                            methodName +" "+parameterSymbol.RefKind+" "+parameterSymbol.Type.ToFullyQualifiedString()+" "+parameterSymbol.Name));
                    }
                }
            }
        
            
        }
        
    }
    
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create( DiagnosticDescriptors.MustBePartial, DiagnosticDescriptors.NestedNotAllow,DiagnosticDescriptors.UnresolvableParameter,DiagnosticDescriptors.NonPublicMethodInjections);
}