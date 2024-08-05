namespace Presolver;

public interface IScope
{
    public Scope Scope { get; }

    public struct Singleton : IScope
    {
        public Scope Scope => Scope.Singleton;
    }

    public struct Transient : IScope
    {
        public Scope Scope => Scope.Transient;
    }

    public struct Scoped : IScope
    {
        public Scope Scope => Scope.Scoped;
    }
}