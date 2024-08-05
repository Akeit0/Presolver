namespace Presolver;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ErrorAttribute(string message) : Attribute;