using System.Diagnostics.CodeAnalysis;
using System.Text;
using BeTiny.Application.Common.Options;
using BeTiny.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BeTiny.IOC.DependencyInjections;

[ExcludeFromCodeCoverage]
public static class ConfigureOptions
{
    /// <summary>
    /// Registers options configuration from app settings.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection RegisterOptions(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.ConfigureReservedAliasOptions(configuration);
        services.ConfigureJwtOptions(configuration);

        return services;
    }

    private static void ConfigureReservedAliasOptions(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var reservedAliasOptionsSection = configuration.GetSection("ReservedAliasPolicy");
        var reservedAliasOptions = reservedAliasOptionsSection.Get<ReservedAliasOptions>()?.Aliases;

        if (
            reservedAliasOptions == null ||
                reservedAliasOptions.Any(
                    x => x is null || ShortUrl.CustomAliasRegex().IsMatch(x.Trim()) is false
                )
        )
        {
            throw new InvalidOperationException(
                "ReservedAliases configuration is invalid. Ensure all aliases are non-empty and " +
                    "match the required pattern.",
                new ArgumentException(
                    "One or more reserved aliases are invalid. Each entry must be non-empty and " +
                        "match the required pattern."
                )
            );
        }

        services.Configure<ReservedAliasOptions>(reservedAliasOptionsSection);
    }

    private static void ConfigureJwtOptions(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var jwtOptionsSection = configuration.GetSection("Jwt");
        var jwtOptions = jwtOptionsSection.Get<JwtOptions>();

        if (
            jwtOptions == null ||
                string.IsNullOrWhiteSpace(jwtOptions.Issuer) ||
                string.IsNullOrWhiteSpace(jwtOptions.Audience) ||
                string.IsNullOrWhiteSpace(jwtOptions.SigningKey) ||
                Encoding.UTF8.GetBytes(jwtOptions.SigningKey).Length < 32 ||
                jwtOptions.ExpiryMinutes <= TimeSpan.Zero
        )
        {
            throw new InvalidOperationException(
                "JWT configuration is invalid. Ensure Issuer, Audience, SigningKey are non-empty, "
                    + "SigningKey is of valid length, and ExpiryMinutes is greater than zero.",
                new ArgumentException(
                    "One or more JWT configuration values are invalid. Ensure Issuer, Audience, "
                        + "SigningKey are non-empty, SigningKey is of valid length, and ExpiryMinutes "
                        + "is greater than zero."
                )
            );
        }

        services.Configure<JwtOptions>(jwtOptionsSection);
    }
}
