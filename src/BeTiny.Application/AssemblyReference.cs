using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace BeTiny.Application;

[ExcludeFromCodeCoverage]
public sealed class AssemblyReference
{
    /// <summary>
    /// Gets the assembly containing the Application layer types.
    /// </summary>
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}
