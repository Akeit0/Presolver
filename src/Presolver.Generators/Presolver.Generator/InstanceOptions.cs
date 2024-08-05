#region

using System;

#endregion

namespace Presolver.Generator;

[Flags]
public enum InstanceOptions
{
    None,
    Inject,
    AddToContainer
}