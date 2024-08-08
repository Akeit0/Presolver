#region

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.DotnetRuntime.Extensions;

#endregion

namespace Presolver.Generator;

public sealed class ContainerTypeData
{
    readonly PresolverContext presolverContext;

    List<Resolver>? sortedNodes;

    public ContainerTypeData(ITypeSymbol containerType, Dictionary<ITypeSymbol, ContainerTypeData> containers, PresolverContext presolverContext)
    {
        FullName = containerType.ToFullyQualifiedString();
        this.presolverContext = presolverContext;
        ContainerType = containerType;
        containers[containerType] = this;

        var baseType = containerType.BaseType;
        ITypeSymbol? parentContainerType = null;
        while (baseType != null)
        {
            if (baseType is { IsGenericType: true, ContainingNamespace.Name: "Presolver", OriginalDefinition.MetadataName: "ChildContainer`1", TypeArguments.Length: 1 })
            {
                parentContainerType = baseType.TypeArguments[0];
                break;
            }

            baseType = baseType.BaseType;
        }

        if (parentContainerType != null) Parent = containers.TryGetValue(parentContainerType, out var parent) ? parent : new(parentContainerType, containers, presolverContext);

        void Add(ITypeSymbol key, Resolver value)
        {
            if (InterfaceToRegisteredMethod.TryGetValue(key, out var list))
                list.Add(value);
            else
                InterfaceToRegisteredMethod[key] = [value];
        }

        try
        {
            var containerBaseType = presolverContext.ContainerBaseType;
            Add(containerBaseType, new ContainerSelfResolver(containerBaseType));
            {
                var interfaces = ContainerType.AllInterfaces;
                foreach (var i in interfaces)
                    if (i is { IsGenericType: true } namedTypeSymbol)
                    {
                        if (namedTypeSymbol.ContainingNamespace.Name != "Presolver") continue;
                        var metadataName = namedTypeSymbol.OriginalDefinition.MetadataName;
                        if (!metadataName.StartsWith("IService")) continue;

                        var typeArguments = namedTypeSymbol.TypeArguments;
                        var registerInterfaces = typeArguments.AsSpan().Slice(0, Math.Max(1, typeArguments.Length - 3)).ToImmutableArray();

                        var scope = typeArguments[typeArguments.Length - 1].ToScope();
                        var resolver = new ByNewResolver((INamedTypeSymbol)typeArguments[typeArguments.Length - 2], registerInterfaces, scope, presolverContext);
                        foreach (var registerInterface in registerInterfaces) Add(registerInterface, resolver);
                    }
            }

            {
                var members = ContainerType.GetMembers();
                foreach (var member in members)
                {
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
                                        var resolver = new ByFactoryResolver((INamedTypeSymbol)typeArguments[typeArguments.Length - 2], "", method, registerInterfaces, scope);
                                        foreach (var registerInterface in registerInterfaces) Add(registerInterface, resolver);
                                    }
                                    else if (metadataName.TryGetScope(out var scope))
                                    {
                                        var typeArguments = namedTypeSymbol.TypeArguments;
                                        var registerInterfaces = typeArguments.AsSpan().Slice(0, Math.Max(1, typeArguments.Length - 2)).ToImmutableArray();
                                        var resolver = new ByFactoryResolver((INamedTypeSymbol)typeArguments[typeArguments.Length - 1], "", method, registerInterfaces, scope);
                                        foreach (var registerInterface in registerInterfaces) Add(registerInterface, resolver);
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
                                    var resolver = new ByInstanceResolver((INamedTypeSymbol)typeArguments[typeArguments.Length - 2], property.Name + ".Value", registerInterfaces, options, presolverContext);
                                    foreach (var registerInterface in registerInterfaces) Add(registerInterface, resolver);
                                    continue;
                                }
                                else if (metadataName.TryGetScope(out var scope))
                                {
                                    if (scope != Scope.Singleton) continue;
                                    var typeArguments = namedTypeSymbol.TypeArguments;
                                    var registerInterfaces = typeArguments.AsSpan().Slice(0, Math.Max(1, typeArguments.Length - 2)).ToImmutableArray();
                                    var resolver = new ByInstanceResolver((INamedTypeSymbol)typeArguments[typeArguments.Length - 1], property.Name + ".Value", registerInterfaces, options, presolverContext);
                                    foreach (var registerInterface in registerInterfaces) Add(registerInterface, resolver);
                                    continue;
                                }
                            }

                            {
                                var resolver = new ByInstanceResolver(returnType, property.Name, ImmutableArray.Create(returnType), options, presolverContext);
                                Add(returnType, resolver);
                            }
                        }


                        else if (attributes.Any(x =>
                                     x.AttributeClass?.OriginalDefinition.MetadataName is "ModuleObject" or "ModuleObjectAttribute"))
                        {
                            var type = property.Type;
                            var module = presolverContext.GetOrAddModuleData(type);
                            //if (module is null) break;
                            module.AddResolvers(property.Name + ".", InterfaceToRegisteredMethod, presolverContext);
                        }
                    }
                }
            }
            {
            }

            if (Parent != null)
                foreach (var pair in Parent.InterfaceToCollectionMethod)
                {
                    var parentMethodList = new List<ByFromParentResolver>();
                    var p = Parent;
                    var depth = 1;
                    while (p != null)
                    {
                        if (p.InterfaceToRegisteredMethod.TryGetValue(pair.Key, out var l))
                            foreach (var m in l)
                                parentMethodList.Add(new(m, depth));

                        depth++;
                        p = p.Parent;
                    }

                    var enumerableType = presolverContext.IEnumerableType.Construct(pair.Key);
                    var listType = presolverContext.IReadOnlyListType.Construct(pair.Key);
                    if (InterfaceToRegisteredMethod.TryGetValue(pair.Key, out var list))
                    {
                        InterfaceToMethod[pair.Key] = list[list.Count - 1];

                        for (var i = 0; i < list.Count(); i++) list[i].Id = i;
                    }

                    list ??= [];
                    var collectionResolver = new CollectionResolver(enumerableType, ImmutableArray.Create((ITypeSymbol)enumerableType), list, parentMethodList);
                    InterfaceToCollectionMethod[pair.Key] = collectionResolver;
                    InterfaceToMethod[enumerableType] = collectionResolver;
                    InterfaceToMethod[listType] = collectionResolver;
                }

            foreach (var pair in InterfaceToRegisteredMethod)
            {
                if (InterfaceToMethod.ContainsKey(pair.Key)) continue;
                InterfaceToMethod[pair.Key] = pair.Value[pair.Value.Count - 1];
                var enumerableType = presolverContext.IEnumerableType.Construct(pair.Key);
                var listType = presolverContext.IReadOnlyListType.Construct(pair.Key);
                var collectionResolver = new CollectionResolver(enumerableType, ImmutableArray.Create((ITypeSymbol)enumerableType), pair.Value, []);
                for (var i = 0; i < pair.Value.Count; i++) pair.Value[i].Id = i;

                InterfaceToCollectionMethod[pair.Key] = collectionResolver;
                InterfaceToMethod[enumerableType] = collectionResolver;
                InterfaceToMethod[listType] = collectionResolver;
            }
        }
        catch (Exception e)
        {
            Exceptions.Add(e);;
        }
    }

    public ITypeSymbol ContainerType { get; }

    public Dictionary<ITypeSymbol, List<Resolver>> InterfaceToRegisteredMethod { get; } = new(SymbolEqualityComparer.Default);

    public Dictionary<ITypeSymbol, CollectionResolver> InterfaceToCollectionMethod { get; } = new(SymbolEqualityComparer.Default);

    public Dictionary<ITypeSymbol, Resolver> InterfaceToMethod { get; } = new(SymbolEqualityComparer.Default);

    public bool IsResolved => sortedNodes != null;

    public string FullName { get; }


    public ContainerTypeData? Parent { get; set; }

    public List<Exception> Exceptions { get; set; }= new();


    public bool TryGetResolveMethod(ITypeSymbol interfaceType, out Resolver? resolveMethod, ref int depth)
    {
        depth++;
        if (InterfaceToMethod.TryGetValue(interfaceType, out var m))
        {
            resolveMethod = new ByFromParentResolver(m, depth);
            return true;
        }

        if (Parent != null) return Parent.TryGetResolveMethod(interfaceType, out resolveMethod, ref depth);

        resolveMethod = null;
        return false;
    }

    public void Resolve()
    {
        if (IsResolved) return;
        foreach (var pair in InterfaceToMethod) pair.Value.ResolveSelf(InterfaceToMethod, Parent);

        foreach (var pair in InterfaceToRegisteredMethod)
            if (pair.Value.Count > 1)
                foreach (var resolveMethod in pair.Value)
                    resolveMethod.ResolveSelf(InterfaceToMethod, Parent);

        foreach (var pair in InterfaceToCollectionMethod) pair.Value.ResolveSelf(InterfaceToMethod, Parent);

        var nodeList = new List<Resolver>();
        foreach (var resolver in InterfaceToMethod.Values)
        {
            if (resolver == null) throw new NullReferenceException("resolver==null");

            nodeList.Add(resolver);
            if (resolver is CollectionResolver collectionMeta)
                foreach (var m in collectionMeta.Dependencies)
                {
                    if (m == null) throw new NullReferenceException("CollectionMeta.Dependencies.Contains(null)");

                    nodeList.Add(m);
                }
        }

        nodeList = nodeList.Distinct().Where(x => x != null).ToList();

        var stack = presolverContext.DependencyStack;
        stack.Clear();
        TopologicalSort.TrySort(nodeList, stack, out sortedNodes);
        stack.Clear();
    }


    public void Write(CodeWriter writer, CodeWriter scopedWriter)
    {
        writer.AppendLine("#pragma warning disable CS1998");
        if (ContainerType.ContainingNamespace is { IsGlobalNamespace: false })
        {
            writer.Append("namespace ");
            writer.AppendLine(ContainerType.ContainingNamespace.FullName());
            writer.BeginBlock();
        }

        writer.Append("partial class ");
        writer.Append(ContainerType.NameWithGenerics());
        var interfaceName = ContainerType.ToFullyQualifiedString() + ".IInterface";
        writer.Append(" : ");
        writer.AppendLine(interfaceName);
        writer.BeginBlock();
        writer.Append("public interface IInterface");

        var first = true;
        if (Parent != null)
        {
            writer.Append(" : ");
            writer.Append(Parent.ContainerType.ToFullyQualifiedString());
            writer.Append(".IInterface");
            first = false;
        }

        foreach (var pair in InterfaceToMethod)
        {
            var type = pair.Key;
            if (pair.Value is CollectionResolver or ContainerSelfResolver) continue;
            if (!first)
            {
                writer.Append(",");
            }
            else
            {
                writer.Append(" : ");
                first = false;
            }

            writer.Append(" Presolver.IResolver<");
            writer.Append(type.ToFullyQualifiedString());
            writer.Append(">");
        }

        writer.AppendLine("");
        writer.BeginBlock();
        writer.EndBlock();
        var disposables = new List<string>();
        List<string> fieldTypes = [];
        {
            writer.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
            writer.AppendLine("__InternalScopedContainer __internalScoped = new (0);");
            writer.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
            writer.AppendLine("public struct __InternalScopedContainer");
            writer.BeginBlock();
            if (Parent != null)
            {
                writer.Append("public ");
                writer.Append(Parent.FullName);
                writer.AppendLine(".__InternalScopedContainer Parent;");
            }


            var fieldCount = 0;
            foreach (var node in sortedNodes!)
            {
                var meta = node;
                if (meta is ByFromParentResolver) continue;
                if (meta.Scope == Scope.Scoped)
                {
                    var typeName = meta.Type.ToFullyQualifiedString();
                    fieldTypes.Add(typeName);
                    var fieldTypeName = meta.UsableTypeName;
                    writer.Append("Presolver.ManualLazy<");
                    writer.Append(typeName);
                    writer.Append("> ");
                    writer.Append("field_");
                    writer.Append(fieldCount);
                    writer.AppendLine(";");
                    writer.Append("public ");
                    writer.Append(typeName);
                    writer.Append(" Resolve_");
                    writer.Append(fieldTypeName);
                    writer.Append("<TContainer>(TContainer container)where TContainer : global::Presolver.ContainerBase, ");
                   // writer.Append(FullName);
                   // writer.Append(" c) where TContainer : global::Presolver.ContainerBase,");
                    writer.AppendLine(interfaceName);
                    writer.BeginBlock();
                    writer.Append("ref var l = ref field_");
                    writer.Append(fieldCount);
                    writer.AppendLine(";");
                    writer.AppendLine("if (!l.TryGetValue(out var v)) lock (l.LockObject) if (!l.TryGetValue(out v))");
                    writer.BeginBlock();

                    writer.Append("l.Value = v = ");
                    meta.WriteCode(writer);
                    writer.AppendLine(";");


                    writer.EndBlock();
                    writer.AppendLine("return v;");
                    writer.EndBlock();

                    if (presolverContext.IsDisposable(meta.Type))
                        if (node is not ByInstanceResolver fromInstanceMeta || (fromInstanceMeta.Options & InstanceOptions.AddToContainer) != 0)
                            disposables.Add("field_" + fieldCount);

                    fieldCount++;
                }
            }

            writer.Append("public __InternalScopedContainer(int dummy)");
            writer.BeginBlock();
            if (Parent != null) writer.AppendLine("Parent = new (0);");

            for (var index = 0; index < fieldTypes.Count; index++)
            {
                var t = fieldTypes[index];
                writer.Append("field_");
                writer.Append(index.ToString());
                writer.Append(" = Presolver.ManualLazy.Create<");
                writer.Append(t);
                writer.AppendLine(">();");
            }

            writer.EndBlock();

            writer.AppendLine("public void Dispose()");
            writer.BeginBlock();
            if (Parent != null) writer.AppendLine("Parent.Dispose();");

            foreach (var d in disposables)
            {
                writer.Append("global::Presolver.ManualLazy.Dispose(ref ");
                writer.Append(d);
                writer.AppendLine(");");
            }

            writer.EndBlock();
            writer.EndBlock();
        }
        fieldTypes.Clear();
        {
            writer.AppendLine("[global::System.ComponentModel. EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");

            writer.AppendLine("__InternalContainer __internalContainer;");

            writer.AppendLine("[global::System.ComponentModel. EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
            writer.AppendLine("public class __InternalContainer:Presolver.IInternalContainer");
            writer.BeginBlock();

            disposables.Clear();

            writer.Append(FullName);
            writer.AppendLine("  c;");
            if (Parent != null)
            {
                writer.Append("public ");
                writer.Append(Parent.FullName);
                writer.AppendLine(".__InternalContainer Parent;");
            }
            writer.AppendLine("public void Initialize(Presolver.ContainerBase container)");
            writer.BeginBlock();
            writer.Append("c = (");
            writer.Append(FullName);
            writer.AppendLine(")container;");
            writer.EndBlock();
            writer.AppendLine("public void SetParent(Presolver.IInternalContainer parent)");
            writer.BeginBlock();
            if(Parent!=null)
            {
                writer.Append("Parent = (");
                writer.Append(Parent.FullName);
                writer.AppendLine(".__InternalContainer)parent;");
            }
            writer.EndBlock();
            var fieldCount = 0;
            foreach (var meta in sortedNodes!)
            {
                if (meta is ByFromParentResolver or ContainerSelfResolver) continue;

                if (meta.Scope != Scope.Singleton) continue;
                var typeName = meta.Type.ToFullyQualifiedString();
                fieldTypes.Add(typeName);
                var fieldTypeName = meta.UsableTypeName;
                writer.Append("Presolver.ManualLazy<");
                writer.Append(typeName);
                writer.Append("> ");
                writer.Append("field_");
                writer.Append(fieldCount);
                writer.AppendLine(";");
                writer.Append("public ");
                writer.Append(typeName);
                writer.Append(" Resolve_");
                writer.Append(fieldTypeName);
                writer.AppendLine("(object _)");

                writer.BeginBlock();
                writer.Append("ref var l = ref field_");
                writer.Append(fieldCount);
                writer.AppendLine(";");
                writer.AppendLine("if (!l.TryGetValue(out var v)) lock (l.LockObject) if (!l.TryGetValue(out v))");
                writer.BeginBlock();
                writer.Append("l.Value = v = ");
                meta.WriteCode(writer,FullName);
                writer.AppendLine(";");
                writer.EndBlock();
                writer.AppendLine("return v;");
                writer.EndBlock();

                if (presolverContext.IsDisposable(meta.Type))
                    if (meta is not ByInstanceResolver fromInstanceMeta || (fromInstanceMeta.Options & InstanceOptions.AddToContainer) != 0)
                        disposables.Add("field_" + fieldCount);

                fieldCount++;
            }

            writer.Append("public __InternalContainer()");
            writer.BeginBlock();

            for (var index = 0; index < fieldTypes.Count; index++)
            {
                var t = fieldTypes[index];
                writer.Append("field_");
                writer.Append(index.ToString());
                writer.Append(" = Presolver.ManualLazy.Create<");
                writer.Append(t);
                writer.AppendLine(">();");
            }

            writer.EndBlock();
            

            foreach (var meta in sortedNodes!)
            {
                if (meta is ByFromParentResolver or CollectionResolver) continue;
                if (meta.Scope != Scope.Transient) continue;
                var typeName = meta.Type.ToFullyQualifiedString();
                writer.Append("public ");
                writer.Append(typeName);
                writer.Append(" ");
                writer.Append("Resolve_");
                writer.Append(meta.UsableTypeName);
                writer.Append("<TContainer>(TContainer container) where TContainer : global::Presolver.ContainerBase,");
                writer.AppendLine(interfaceName);
                writer.BeginBlock();

                writer.Append("return ");
                meta.WriteCode(writer);
                writer.AppendLine(";");
                writer.EndBlock();
            }


            writer.AppendLine("public void Dispose()");
            writer.BeginBlock();

            foreach (var d in disposables)
            {
                writer.Append("global::Presolver.ManualLazy.Dispose(ref ");
                writer.Append(d);
                writer.AppendLine(");");
            }

            writer.EndBlock();
        }
        writer.EndBlock();


        var depth = 0;
        var set = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        scopedWriter.Indent = writer.Indent + 1;
        for (var i = 0; i < writer.Indent + 1; i++) scopedWriter.AppendIndented("");


        void Append(string n)
        {
            writer.Append(n);
            scopedWriter.Append(n);
        }

        void AppendLine(string n)
        {
            writer.AppendLine(n);
            scopedWriter.AppendLine(n);
        }

        void BeginBlock()
        {
            writer.BeginBlock();
            scopedWriter.BeginBlock();
        }

        void EndBlock()
        {
            writer.EndBlock();
            scopedWriter.EndBlock();
        }
        writer.AppendLine("protected override Presolver.IInternalContainer InternalContainer => __internalContainer ??=new ();");
        scopedWriter.AppendLine("protected override Presolver.IInternalContainer InternalContainer => null;");
        var container = this;
        while (container != null)
        {
            foreach (var pair in container.InterfaceToMethod)
            {
                var interfaceType = pair.Key;
                if (pair.Value is CollectionResolver or ContainerSelfResolver) continue;
                if (!set.Add(interfaceType)) continue;

                var typeName = interfaceType.ToFullyQualifiedString();

                Append(typeName);
                Append(" Presolver.IResolver<");
                Append(typeName);
                AppendLine(">.Resolve()");
                BeginBlock();
                Append("return ");

                static void Add(CodeWriter writer, CodeWriter subWriter, Resolver meta, int depth)
                {
                    void Append(string n)
                    {
                        writer.Append(n);
                        subWriter.Append(n);
                    }

                    if (meta.Scope == Scope.Scoped) Append("__internalScoped.");
                    else
                    {
                        subWriter.Append("Parent.");
                        Append("__internalContainer.");
                      
                    }

                    for (var d = 0; d < depth; d++) Append("Parent.");
                    
                    Append("Resolve_");
                    Append(meta.UsableTypeName);
                    Append("(this)");
                    // if (depth == 0)
                    // {
                    //     Append("(this)");
                    //     subWriter.Append("(this,this.Parent)");
                    // }
                    // else
                    // {
                    //     Append("(this,this");
                    //     for (var d = 0; d < depth; d++) Append(".Parent");
                    //
                    //     subWriter.Append(".Parent");
                    //
                    //     Append(")");
                    // }
                }

                Add(writer, scopedWriter, pair.Value, depth);
                AppendLine(";");
                EndBlock();
                Append("global::System.Collections.Generic.List<");
                Append(typeName);
                Append("> Presolver.IResolver<");
                Append(typeName);
                AppendLine(">.ResolveAll(bool includeParentSingletons)");
                BeginBlock();
                Append("var list = new global::System.Collections.Generic.List<");
                Append(typeName);
                AppendLine(">();");

                var d = InterfaceToCollectionMethod[interfaceType];
                foreach (var m in d.Dependencies)
                    if (m.Scope == Scope.Singleton)
                    {
                        scopedWriter.Append("if(includeParentSingletons)");
                        var depth2 = 0;
                        if (m is ByFromParentResolver parentMeta)
                        {
                            writer.Append("if(includeParentSingletons)");
                            depth2 = parentMeta.Depth;
                        }

                        Append("list.Add(");
                        Add(writer, scopedWriter, m, depth2);
                        AppendLine(");");
                    }
                    else
                    {
                        Append("list.Add(");
                        if (m is ByFromParentResolver parentMeta)
                            Add(writer, scopedWriter, m, parentMeta.Depth);
                        else Add(writer, scopedWriter, m, 0);

                        AppendLine(");");
                    }

                AppendLine("return list;");
                EndBlock();
            }

            container = container.Parent;
            depth++;
        }

        AppendLine("protected override void DisposeFields()");
        BeginBlock();
        writer.AppendLine("__internalContainer.Dispose();");
        AppendLine("__internalScoped.Dispose();");
        EndBlock();
        {
            writer.AppendLine("public ChildScope CreateScope()=> new ChildScope(this);");
            writer.Append("public sealed class ChildScope:global::Presolver.ChildContainer<");
            writer.Append(FullName);
            writer.Append(">, ");
            writer.Append(FullName);
            writer.AppendLine(".IInterface");
            writer.BeginBlock();
            writer.Append("public ChildScope(");
            writer.Append(FullName);
            writer.AppendLine(" container):base(container)");
            writer.BeginBlock();
            writer.EndBlock();
            writer.AppendLine("[global::System.ComponentModel. EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
            writer.AppendLine("__InternalScopedContainer __internalScoped= new (0);");

            writer.Append(scopedWriter);
            writer.AppendLine("");
            writer.EndBlock();
            writer.EndBlock();
            if (ContainerType.ContainingNamespace is { IsGlobalNamespace: false }) writer.EndBlock();
        }

        {
            writer.AppendLine("/*");
            writer.AppendLine("SortedNodes:");
            foreach (var n in sortedNodes)
            {
                var meta = n;
                writer.AppendLine(meta.Type.ToFullyQualifiedString() + " " + n.UsableTypeName);

                if (meta.Dependencies.Length > 0)
                {
                    writer.BeginBlock();
                    var typeDependencies = meta.TypeDependencies;
                    var dependencies = meta.Dependencies;
                    if (typeDependencies.Length == dependencies.Length)
                    {
                        var length = dependencies.Length;
                        for (var i = 0; i < length; i++)
                        {
                            writer.Append(dependencies[i].Type.ToFullyQualifiedString());
                            writer.Append(" As ");
                            writer.AppendLine(typeDependencies[i].ToFullyQualifiedString());
                        }
                    }
                    else
                    {
                        foreach (var m in meta.Dependencies)
                            writer.AppendLine(m.UsableTypeName);
                    }

                    writer.EndBlock();
                }
            }

            writer.AppendLine("*/");
            writer.AppendLine("");
        }
    }

    public void WriteFallBack(CodeWriter writer, CodeWriter scopedWriter)
    {
        writer.AppendLine("#pragma warning disable CS1998");
        if (ContainerType.ContainingNamespace is { IsGlobalNamespace: false })
        {
            writer.Append("namespace ");
            writer.AppendLine(ContainerType.ContainingNamespace.FullName());
            writer.BeginBlock();
        }

        foreach (var exception in Exceptions)
        {
            writer.Append("[global::Presolver.Error(\"");
            writer.Append(exception is PresolverGeneratorException generatorException ? generatorException.FormatedMessage : exception.Message);
            writer.AppendLine("\")]");
        }
       
        writer.Append("partial class ");
        writer.Append(ContainerType.NameWithGenerics());
        var interfaceName = ContainerType.ToFullyQualifiedString() + ".IInterface";
        writer.Append(" : ");
        writer.AppendLine(interfaceName);
        writer.BeginBlock();
        writer.Append("public interface IInterface");

        var first = true;
        if (Parent != null)
        {
            writer.Append(" : ");
            writer.Append(Parent.ContainerType.ToFullyQualifiedString());
            writer.Append(".IInterface");
            first = false;
        }

        foreach (var pair in InterfaceToMethod)
        {
            var type = pair.Key;
            if (pair.Value is CollectionResolver) continue;
            if (!first)
            {
                writer.Append(",");
            }
            else
            {
                writer.Append(" : ");
                first = false;
            }

            writer.Append(" Presolver.IResolver<");
            writer.Append(type.ToFullyQualifiedString());
            writer.Append(">");
        }

        writer.AppendLine("");
        writer.BeginBlock();
        writer.EndBlock();

        var set = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        scopedWriter.Indent = writer.Indent + 1;
        for (var i = 0; i < writer.Indent + 1; i++) scopedWriter.AppendIndented("");


        void Append(string n)
        {
            writer.Append(n);
            scopedWriter.Append(n);
        }

        void AppendLine(string n)
        {
            writer.AppendLine(n);
            scopedWriter.AppendLine(n);
        }

        void BeginBlock()
        {
            writer.BeginBlock();
            scopedWriter.BeginBlock();
        }

        void EndBlock()
        {
            writer.EndBlock();
            scopedWriter.EndBlock();
        }

        var container = this;
        while (container != null)
        {
            foreach (var pair in container.InterfaceToMethod)
            {
                var interfaceType = pair.Key;
                if (pair.Value is CollectionResolver) continue;
                if (!set.Add(interfaceType)) continue;

                var typeName = interfaceType.ToFullyQualifiedString();

                Append(typeName);
                Append(" Presolver.IResolver<");
                Append(typeName);
                AppendLine(">.Resolve()");
                BeginBlock();
                Append("return default");
                AppendLine(";");
                EndBlock();
                Append("global::System.Collections.Generic.List<");
                Append(typeName);
                Append("> Presolver.IResolver<");
                Append(typeName);
                AppendLine(">.ResolveAll(bool includeParentSingletons)");
                BeginBlock();
                Append("var list = new global::System.Collections.Generic.List<");
                Append(typeName);
                AppendLine(">();");
                AppendLine("return default;");
                EndBlock();
            }

            container = container.Parent;
        }

        // AppendLine("protected override void DisposeFields()");
        // BeginBlock();
        // AppendLine("__internalScoped.Dispose();");
        // EndBlock();
        {
            writer.AppendLine("public ChildScope CreateScope()=> new ChildScope(this);");
            writer.Append("public sealed class ChildScope:global::Presolver.ChildContainer<");
            writer.Append(FullName);
            writer.Append(">, ");
            writer.Append(FullName);
            writer.AppendLine(".IInterface");
            writer.BeginBlock();
            writer.Append("public ChildScope(");
            writer.Append(FullName);
            writer.AppendLine(" container):base(container)");
            writer.BeginBlock();
            writer.EndBlock();
            writer.Append(scopedWriter);
            writer.AppendLine("");
            writer.EndBlock();
            writer.EndBlock();
            if (ContainerType.ContainingNamespace is { IsGlobalNamespace: false }) writer.EndBlock();
        }
    }
}