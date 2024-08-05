namespace Presolver;

[AttributeUsage(AttributeTargets.Property)]
public sealed class InstanceAttribute : Attribute
{
    public InstanceAttribute(InstanceOptions fromInstanceOptions = InstanceOptions.None)
    {
    }
}