using Presolver;

using var container = new Container();
var d = new InstanceD("SomeName");
using var childContainer = new ChildContainer(container, d);
Console.WriteLine(container.Resolve<B>());
Console.WriteLine(container.Resolve<B>());
Console.WriteLine("----------------------------------");
Console.WriteLine(childContainer.Resolve<B>());
Console.WriteLine(childContainer.Resolve<C>());
Console.WriteLine(childContainer.Resolve<IAInterface>());

Console.WriteLine("----------------------------------");
using var s = childContainer.CreateScope();
Console.WriteLine(s.Resolve<B>());



// B {a: {A {id:0}, A {id:1}} ,i:1}
// B {a: {A {id:0}, A {id:2}} ,i:1}
// ----------------------------------
// B {a: {A {id:3}, A {id:4}} ,i:2}
// C { b:B {a: {A {id:3}, A {id:5}} ,i:2}, a:A {id:6} ,d:InstanceD { name:SomeName, b:B {a: {A {id:3}, A {id:7}} ,i:2}}}
// A {id:8}
// ----------------------------------
//     B {a: {A {id:9}, A {id:10}} ,i:2}
// Dispose A {id:9}
// Dispose InstanceD { name:SomeName, b:B {a: {A {id:3}, A {id:7}} ,i:2}}
// Dispose A {id:3}
// Dispose A {id:0}


public interface IAInterface;

public partial class A : IAInterface, IDisposable
{
    static int _counter;

    readonly int id = _counter++;
}

public partial record B(IReadOnlyList<IAInterface> a, int i);

public partial record C( B b, IAInterface a,InstanceD d);

public partial class InstanceD(string name) : IDisposable
{
    B? b;

    [Inject]
    public void Ctor(B b)
    {
        this.b = b;
    }
}

[GenerateResolver]
public sealed partial class Container : ContainerBase, IScoped<IAInterface, A>, ITransient<B, B>
{
    [Factory] Transient<int> GetInt() => 1;
    [Factory] Transient<IAInterface, A> GetA() => new A();
}


[GenerateResolver]
public sealed partial class ChildContainer(Container c, InstanceD d) : ChildContainer<Container>(c), ISingleton<C>
{
    [Factory] Transient<int> GetInt() => 2;

    [Instance(InstanceOptions.Inject | InstanceOptions.AddToContainer)]
    Singleton<InstanceD> InstanceD => d;
    
}


partial class A
{
    public override string ToString()
    {
        return $"A {{id:{id}}}";
    }

    public void Dispose()
    {
        Console.WriteLine($"Dispose {this}");
    }
}

partial class InstanceD
{
    public override string ToString()
    {
        return $"InstanceD {{ name:{name}, b:{b}}}";
    }

    public void Dispose()
    {
        Console.WriteLine($"Dispose {this}");
    }
}

partial record B
{
    public override string ToString()
    {
        return $"B {{a: {{{string.Join(", ", a)}}} ,i:{i}}}";
    }
}

partial record C
{
    public override string ToString()
    {
        return $"C {{ b:{b}, a:{a} ,d:{d}}}";
    }
}