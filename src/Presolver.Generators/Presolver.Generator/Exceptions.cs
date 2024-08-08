#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

#endregion

namespace Presolver.Generator;

public abstract class PresolverGeneratorException(string message) : Exception(message)
{
    public abstract string FormatedMessage { get; }
}

public class CircularDependencyException(string message) : PresolverGeneratorException(message)
{
    public override string FormatedMessage => "Circular dependency detected in " + Message;

    public static CircularDependencyException Create(List<(Resolver, int )> nodes)
    {
        var builder = new StringBuilder();
        var isFirst = true;
        foreach (var (node, index) in nodes)
        {
            if (isFirst)
                isFirst = false;
            else
                builder.Append(" -> ");

            node.WriteDebugInfo(builder);
            builder.Append("[");
            builder.Append(index);
            builder.Append("]");
        }

        return new(builder.ToString());
    }
}

public class UnregisteredTypeException(string message) : PresolverGeneratorException(message)
{
    public override string FormatedMessage => "Unregistered type: " + Message;
}

public class UnresolvableParameterException(string message,IParameterSymbol parameterSymbol) : PresolverGeneratorException(message)
{
    public override string FormatedMessage => "Unresolvable param: " + Message;
    public Location? Location => parameterSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation();
    
}