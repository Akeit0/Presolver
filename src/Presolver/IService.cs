namespace Presolver;

public interface IService<T, TScope> where TScope : struct, IScope;

public interface IService<TInterface, T, TScope> where T : TInterface where TScope : struct, IScope;

public interface IService<TInterface0, TInterface1, T, TScope> where T : TInterface0, TInterface1 where TScope : struct, IScope;

public interface IService<TInterface0, TInterface1, TInterface2, T, TScope> where T : TInterface0, TInterface1, TInterface2 where TScope : struct, IScope;

public interface IService<TInterface0, TInterface1, TInterface2, TInterface3, T, TScope> where T : TInterface0, TInterface1, TInterface2, TInterface3 where TScope : struct, IScope;

public interface IService<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, T, TScope> where T : TInterface0, TInterface1, TInterface2, TInterface3, TInterface4 where TScope : struct, IScope;

public interface IService<TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, T, TScope> where T : TInterface0, TInterface1, TInterface2, TInterface3, TInterface4, TInterface5 where TScope : struct, IScope;