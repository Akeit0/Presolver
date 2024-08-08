namespace Presolver;





public interface IInternalContainer
{
    void Initialize(ContainerBase container);
    
    void SetParent(IInternalContainer parent);
    //void Dispose();
}