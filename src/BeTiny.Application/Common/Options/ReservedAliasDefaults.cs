namespace BeTiny.Application.Common.Options;

/// <summary>
/// Represents the default reserved aliases used in the application.
/// </summary>
public sealed class ReservedAliasDefaults
{
    /// <summary>
    /// Gets or sets the default reserved aliases used in the application.
    /// </summary>
    /// <value>A read-only collection of reserved aliases.</value>
    public IReadOnlyCollection<string> Aliases { get; } = [
        "admin",
        "login",
        "dashboard",
        "api",
        "auth",
        "health",
        "swagger",
        "docs"
    ];
}
