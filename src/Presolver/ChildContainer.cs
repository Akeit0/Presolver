namespace Presolver;

public abstract class ChildContainer<T> : ContainerBase where T : ContainerBase,IUserDeclaredContainer
{
    protected ChildContainer(T parent)
    {
        
        
        Parent = parent;
        
        // ReSharper disable once VirtualMemberCallInConstructor
        InternalContainer?.SetParent(GetInternalContainer(parent)!);
        parent.AddDisposable(this);
    }

    public T Parent { get; }
}