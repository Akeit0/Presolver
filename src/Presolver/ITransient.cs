namespace Presolver;

public interface ITransient<T> : IService<T, IScope.Transient>;

public interface ITransient<TInterface, T> : IService<TInterface, T, IScope.Transient> where T : TInterface;

public interface ITransient<TInterface0, TInterface1, T> : IService<TInterface0, TInterface1, T, IScope.Transient> where T : TInterface0, TInterface1;

public interface ITransient<TInterface0, TInterface1, TInterface2, T> : IService<TInterface0, TInterface1, TInterface2, T, IScope.Transient> where T : TInterface0, TInterface1, TInterface2;

public interface ITransient<TInterface0, TInterface1, TInterface2, TInterface3, T> : IService<TInterface0, TInterface1, TInterface2, TInterface3, T, IScope.Transient> where T : TInterface0, TInterface1, TInterface2, TInterface3;

public interface ITransient<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, T> : IService<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, T, IScope.Transient> where T : TInterface0, TInterface1, TInterface2, TInterface3, TInterface4;

public interface ITransient<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, T> : IService<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, T, IScope.Transient> where T : TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, TInterface5;