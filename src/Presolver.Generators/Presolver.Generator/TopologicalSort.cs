#region

using System;
using System.Collections.Generic;

#endregion

namespace Presolver.Generator;

public static class TopologicalSort
{
    public static bool TrySort(List<Resolver> nodes, List<(Resolver, int )> dependency, out List<Resolver> result)
    {
        result = [];
        var visited = new Dictionary<Resolver, bool>();

        foreach (var node in nodes)
            if (!Visit(node, dependency, visited, result))
                return false;

        return true;
    }

    static bool Visit(Resolver node, List<(Resolver, int )> dependency, Dictionary<Resolver, bool> seenNodes, List<Resolver> sortedNodes)
    {
        if (node == null) throw new NullReferenceException("node == null");

        if (seenNodes.TryGetValue(node, out var isTmp))
        {
            if (isTmp) throw CircularDependencyException.Create(dependency);
            if (dependency.Count != 0) dependency.RemoveAt(dependency.Count - 1);
            return true;
        }

        if (node.Dependencies.Length == 0)
        {
            seenNodes[node] = false;
            sortedNodes.Add(node);
            if (dependency.Count != 0) dependency.RemoveAt(dependency.Count - 1);
            return true;
        }

        seenNodes[node] = true;

        for (var index = 0; index < node.Dependencies.Length; index++)
        {
            var to = node.Dependencies[index];
            dependency.Add((node, index));
            if (!Visit(to, dependency, seenNodes, sortedNodes)) return false;
        }

        seenNodes[node] = false;
        sortedNodes.Add(node);
        if (dependency.Count != 0) dependency.RemoveAt(dependency.Count - 1);
        return true;
    }
}