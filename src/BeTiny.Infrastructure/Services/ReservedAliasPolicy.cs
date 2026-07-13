using BeTiny.Application.Common.Options;
using BeTiny.Domain.Common.Interfaces;
using BeTiny.Domain.Entities;
using Microsoft.Extensions.Options;

namespace BeTiny.Infrastructure.Services;

/// <inheritdoc />
public sealed class ReservedAliasPolicy : IReservedAliasPolicy
{
    private readonly HashSet<string> _reservedAliases;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReservedAliasPolicy"/> class.
    /// </summary>
    /// <param name="aliasDefaults">The default reserved aliases.</param>
    /// <param name="aliasOptions">The reserved alias options.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when an entry in <paramref name="aliasOptions"/> is invalid after trimming.
    /// </exception>
    public ReservedAliasPolicy(
        ReservedAliasDefaults aliasDefaults,
        IOptions<ReservedAliasOptions> aliasOptions
    )
    {
        _reservedAliases = new HashSet<string>(aliasDefaults.Aliases, StringComparer.OrdinalIgnoreCase);

        foreach (var alias in aliasOptions.Value.Aliases)
        {
            var trimmed = alias.Trim();

            if (!ShortUrl.CustomAliasRegex().IsMatch(trimmed))
            {
                throw new InvalidOperationException(
                    "Reserved alias options contain an invalid entry.",
                    new ArgumentException(
                        $"Invalid reserved alias: '{alias}'. Each entry must be non-empty and " +
                            "match the pattern ^[A-Za-z0-9_-]{3,50}$."
                    )
                );
            }

            _reservedAliases.Add(trimmed);
        }
    }

    /// <inheritdoc />
    public bool IsReserved(string alias) => string.IsNullOrWhiteSpace(alias) is false &&
        _reservedAliases.Contains(alias);
}
