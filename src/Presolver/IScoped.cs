namespace Presolver;

public interface IScoped<T> : IService<T, IScope.Scoped>;

public interface IScoped<TInterface, T> : IService<TInterface, T, IScope.Scoped> where T : TInterface;

public interface IScoped<TInterface0, TInterface1, T> : IService<TInterface0, TInterface1, T, IScope.Scoped> where T : TInterface0, TInterface1;

public interface IScoped<TInterface0, TInterface1, TInterface2, T> : IService<TInterface0, TInterface1, TInterface2, T, IScope.Scoped> where T : TInterface0, TInterface1, TInterface2;

public interface IScoped<TInterface0, TInterface1, TInterface2, TInterface3, T> : IService<TInterface0, TInterface1, TInterface2, TInterface3, T, IScope.Scoped> where T : TInterface0, TInterface1, TInterface2, TInterface3;

public interface IScoped<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, T> : IService<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, T, IScope.Scoped> where T : TInterface0, TInterface1, TInterface2, TInterface3, TInterface4;

public interface IScoped<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, T> : IService<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, T, IScope.Transient> where T : TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, TInterface5;