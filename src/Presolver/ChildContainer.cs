namespace Presolver;

public abstract class ChildContainer<T> : ContainerBase, IChildContainer where T : ContainerBase
{
    protected ChildContainer(T parent)
    {
        Parent = parent;
        // Console.WriteLine("new ChildContainer "+GetType()+" "+parent);
        parent.AddDisposable(this);
    }

    public T Parent { get; }

    ContainerBase IChildContainer.Parent => Parent;
}