// ReSharper disable TypeParameterCanBeVariant

namespace Presolver;

public interface IResolver<T>
{
    T Resolve();

    void ResolveAll(List<T> list,bool includeParentSingletons = true);
    
}