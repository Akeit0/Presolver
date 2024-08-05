namespace Presolver;

public interface ISingleton<T> : IService<T, IScope.Singleton>;

public interface ISingleton<TInterface, T> : IService<TInterface, T, IScope.Singleton> where T : TInterface;

public interface ISingleton<TInterface0, TInterface1, T> : IService<TInterface0, TInterface1, T, IScope.Singleton> where T : TInterface0, TInterface1;

public interface ISingleton<TInterface0, TInterface1, TInterface2, T> : IService<TInterface0, TInterface1, TInterface2, T, IScope.Singleton> where T : TInterface0, TInterface1, TInterface2;

public interface ISingleton<TInterface0, TInterface1, TInterface2, TInterface3, T> : IService<TInterface0, TInterface1, TInterface2, TInterface3, T, IScope.Singleton> where T : TInterface0, TInterface1, TInterface2, TInterface3;

public interface ISingleton<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, T> : IService<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, T, IScope.Singleton> where T : TInterface0, TInterface1, TInterface2, TInterface3, TInterface4;

public interface ISingleton<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, T> : IService<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, T, IScope.Transient> where T : TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, TInterface5;