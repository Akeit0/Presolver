#pragma warning disable CS9113 // Parameter is unread.
namespace Presolver;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ErrorAttribute(string message) : Attribute;