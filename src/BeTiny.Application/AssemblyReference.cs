using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace BeTiny.Application;

[ExcludeFromCodeCoverage]
public sealed class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}
