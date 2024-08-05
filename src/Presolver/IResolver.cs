// ReSharper disable TypeParameterCanBeVariant

namespace Presolver;

public interface IResolver<T>
{
    T Resolve();

    List<T> ResolveAll(bool includeParentSingletons = true);

    // void ResolveInto(List<T> list,bool withOutParentSingletons=false);
}