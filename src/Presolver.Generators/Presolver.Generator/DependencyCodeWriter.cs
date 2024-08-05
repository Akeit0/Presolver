namespace Presolver.Generator;

public static class DependencyCodeWriter
{
    public static void Write(CodeWriter writer, Resolver resolver, string containerType = "TInterface", string containerName = "container", bool fromInternal = true, bool first = true)
    {
        if (resolver == null!)
        {
            writer.Append("null");
            return;
        }


        if (resolver is ContainerSelfResolver self) self.WriteCode(writer, fromInternal);
        if (first && resolver is ByInstanceResolver instanceMeta)
        {
            writer.Append("c.");
            writer.Append(instanceMeta.Name);

            if ((instanceMeta.Options & InstanceOptions.Inject) != 0)
            {
                writer.AppendLine(";");
                var end = instanceMeta.WriteCode(writer, fromInternal);
                for (var index = 0; index < resolver.Dependencies.Length; index++)
                {
                    var dependentNode = resolver.Dependencies[index];
                    Write(writer, dependentNode, containerType, containerName, fromInternal, false);
                    if (index != resolver.Dependencies.Length - 1) writer.Append(",");
                }

                writer.Append(end);
            }

            return;
        }

        if (resolver is ByFromParentResolver p)
        {
            var end = resolver.WriteCode(writer, fromInternal);
            writer.Append(fromInternal ? "c" : "this");

            writer.Append(",");
            writer.Append("c");
            for (var i = 0; i < p.Depth; i++) writer.Append(".Parent");

            writer.Append(end);
            return;
        }

        if (resolver.Scope != Scope.Singleton)
        {
            if (resolver is CollectionResolver collectionMeta)
            {
                writer.Append("global::Presolver.ResolveExtensions.ResolveAll<");
                writer.Append(containerType);
                writer.Append(",");
                writer.Append(collectionMeta.ElementType.ToFullyQualifiedString());
                writer.Append(">(");
                writer.Append(containerName);
                writer.Append(')');
            }
            else
            {
                var end = resolver.WriteCode(writer, fromInternal);

                for (var index = 0; index < resolver.TypeDependencies.Length; index++)
                {
                    var dependentNode = resolver.Dependencies[index];
                    if (dependentNode is ContainerSelfResolver selfResolver)
                    {
                        selfResolver.WriteCode(writer, fromInternal);
                    }
                    else if (dependentNode is CollectionResolver collectionMeta2)
                    {
                        writer.Append("global::Presolver.ResolveExtensions.ResolveAll<");
                        writer.Append(containerType);
                        writer.Append(",");
                        writer.Append(collectionMeta2.ElementType.ToFullyQualifiedString());
                        writer.Append(">(");
                        writer.Append(containerName);
                        writer.Append(')');
                    }
                    else
                    {
                        writer.Append("global::Presolver.ResolveExtensions.Resolve<");
                        writer.Append(containerType);
                        writer.Append(",");
                        writer.Append(resolver.TypeDependencies[index].ToFullyQualifiedString());
                        writer.Append(">(");
                        writer.Append(containerName);
                        writer.Append(')');
                    }

                    if (index != resolver.Dependencies.Length - 1) writer.Append(",");
                }

                writer.Append(end);
            }

            return;
        }

        {
            if (!first)
            {
                writer.Append("Resolve_");
                writer.Append(resolver.UsableTypeName);
                writer.Append("(container, c)");
                return;
            }

            var end = resolver.WriteCode(writer, fromInternal);

            for (var index = 0; index < resolver.Dependencies.Length; index++)
            {
                var dependentNode = resolver.Dependencies[index];
                Write(writer, dependentNode, containerType, containerName, fromInternal, false);
                if (index != resolver.Dependencies.Length - 1) writer.Append(",");
            }

            writer.Append(end);
        }
    }
}