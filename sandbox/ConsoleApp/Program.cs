// See https://aka.ms/new-console-template for more information

#region

using Presolver;

#endregion

using var container = new Container();
var service = container.Resolve<IServiceB>();
service.DoSomethingB();


public interface IServiceA
{
    void DoSomething();
}

public class ServiceA : IServiceA, IDisposable
{
    public void Dispose()
    {
        Console.WriteLine("ServiceA Dispose");
    }

    public void DoSomething()
    {
        Console.WriteLine("ServiceA");
    }
}

public interface IServiceB
{
    void DoSomethingB();
}

public class ServiceB(IServiceA a, ContainerBase v) : IServiceB
{
    public void DoSomethingB()
    {
        Console.WriteLine("ServiceB");
    }
}


[GenerateContainer]
public sealed partial class Container : ContainerBase, ISingleton<IServiceA, ServiceA>, ITransient<IServiceB, ServiceB>
{
}