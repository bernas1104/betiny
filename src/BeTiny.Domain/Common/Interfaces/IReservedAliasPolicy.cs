namespace BeTiny.Domain.Common.Interfaces;

/// <summary>
/// Defines a policy for checking if a given alias is reserved.
/// </summary>
public interface IReservedAliasPolicy
{
    /// <summary>
    /// Determines whether the specified alias is reserved.
    /// </summary>
    /// <param name="alias">The alias to check.</param>
    /// <returns><c>true</c> if the alias is reserved; otherwise, <c>false</c>.</returns>
    bool IsReserved(string alias);
}
