#region

using System.Runtime.CompilerServices;

#endregion

namespace Presolver;

public static class ResolveExtensions
{
    public static T Resolve<T>(this IResolver<T> resolver)
    {
        return resolver.Resolve();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Resolve<TContainer, T>(this TContainer resolver) where TContainer : IResolver<T>
    {
        return resolver.Resolve();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<T> ResolveAll<T>(this IResolver<T> resolver, bool includeParentSingletons = true)
    {
        return resolver.ResolveAll(includeParentSingletons);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<T> ResolveAll<TContainer, T>(this TContainer resolver, bool includeParentSingletons = true) where TContainer : IResolver<T>
    {
        return resolver.ResolveAll(includeParentSingletons);
    }
}