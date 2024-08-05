#region

using System;
using System.Collections.Generic;
using System.Text;

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

            node.WriteDebugInfo(builder, index);
        }

        return new(builder.ToString());
    }
}

public class UnregisteredTypeException(string message) : PresolverGeneratorException(message)
{
    public override string FormatedMessage => "Unregistered type: " + Message;
}