namespace Presolver;

[Flags]
public enum InstanceOptions
{
    None,
    Inject,
    AddToContainer,
    Bind = Inject | AddToContainer
}