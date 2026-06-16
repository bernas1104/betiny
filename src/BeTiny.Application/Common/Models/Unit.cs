using System.Diagnostics.CodeAnalysis;

namespace BeTiny.Application.Common.Models;

[ExcludeFromCodeCoverage]
public readonly struct Unit
{
    /// <summary>
    /// Gets the default singleton value of <see cref="Unit"/>.
    /// </summary>
    public static readonly Unit Value = default;
}
