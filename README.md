# Presolver
Presolver is a simple dependency injection container for .NET to resolve dependencies at compile time, powered by Roslyn Source Generators.
Inspired by  [stronginject](https://github.com/YairHalberstadt/stronginject).


## NuGet
https://www.nuget.org/packages/Presolver/0.1.1
```
dotnet add package Presolver --version 0.1.1
```


## How To Use
Define your services and interfaces.  
Define your container class which derives from `ContainerBase`, and implement the interfaces with the services.
You have to mark it partial and attach `GenerateResolver` attribute to it.
```csharp
using Presolver;

interface IServiceA;

class ServiceA : IServiceA;

record ServiceB(IServiceA ServiceA);

[GenerateResolver]
public partial class Container : ContainerBase, ITransient<IServiceA, ServiceA>,
                                                ISingleton<ServiceB>;
```
Then you can resolve the services from the container.
```csharp
using Presolver;
var container = new Container();
var a = container.Resolve<IServiceA>();
var b = container.Resolve<ServiceB>();
//var a2 = container.Resolve<ServiceA>();//Compile error
```

To create a child container, you can define a child container class which derives  the `ChildContainer<parent container class>`.
(By the way, I really like the primary constructor feature of C# 12.)
```csharp
[GenerateResolver]
public partial class ChildContainer(Container parent) : ChildContainer<Container>(parent),
                                                        ITransient<IServiceA, ServiceA>;
```
If you don't have to register additional services, you can use `CreateScope()`.
```csharp
var container = new Container();
var scope = container.CreateScope();
```


## Supported Scopes (Lifetimes)
- Transient  
  Create a new instance every time it is resolved.
- Singleton  
  Create a single instance and reuse it every time it is resolved.  
  The instance will be disposed when the container is disposed.
- Scoped  
  Create a single instance per scope and reuse it within the same scope.  
  The instance will be disposed when the container is disposed.
## Registering Services

### Constructors
If you want to use constructor injection, you can add the interfaces.
```csharp
//Register class A as IService with scoped lifetime.
class Container :  IService<IServiceA,A,IScope.Scoped>;
//or
class Container :  IScoped<IServiceA,A>;
```
### Factory methods
```csharp
public partial class Container 
{
    [Factory] Transient<ServiceB> GetB(IServiceA a) => new (a);
//or [Factory] Transient<ServiceB,ServiceB> GetB(IService a) => new (a);
//or [Factory] Service<ServiceB,IScope.Transient> GetB(IService a) => new (a);
}
```
### Instance (with method injection)

Instance registration is regard as a singleton, but `Dispose` method won't be called by default.
```csharp
public class D(string name) : IDisposable
{
    B? b;
    [Inject]
    public void Ctor(IServiceA a)
    {
        this.b = b;
    }
}
public partial class Container(D d)
{
     //A flag to call injection method| A flag to call `Dispose` on container disposal
     [Instance(InstanceOptions.Inject | InstanceOptions.AddToContainer)]
     Singleton<D> D { get; }= d;
    //or [Instance(InstanceOptions.Inject | InstanceOptions.AddToContainer)]
    //D D => d;
}
```


## Reusability feature
Interfaces are reusable.
```csharp
interface IModuleAB<TScope> : ISingleton<IServiceA, A> ,IService<ServiceB,TScope>where TScope : struct, IScope;
```
Factories and Instances are also reusable.
```csharp
public struct ModuleD<TScope>(D d)where TScope : struct, IScope
{
    [Instance(InstanceOptions.Inject | InstanceOptions.AddToContainer)]
    Singleton<D> D { get; }= d;
    [Factory]
    Service<int,TScope> GetInt ()=> 1;
}

public partial class Container(D d)
{
    [ModuleObject]
    ModuleD<IScope.Singleton> ModuleD { get; }= new (d);
}
```


# LICENSE
MIT