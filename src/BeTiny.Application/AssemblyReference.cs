using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace BeTiny.Application;

[ExcludeFromCodeCoverage]
public sealed class AssemblyReference
{
    public Assembly GetAssembly() => GetType().Assembly;
}
