using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BeTiny.Application.Common.Interfaces.Services;
using BeTiny.Application.Common.Models;
using BeTiny.Application.Common.Options;
using BeTiny.Domain.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BeTiny.Infrastructure.Services;

/// <summary>
/// Provides functionality to issue JSON Web Tokens (JWT) for authentication.
/// </summary>
public sealed class JwtTokenProvider
    : ITokenProvider
{
    private readonly JwtOptions _jwtOptions;
    private readonly IDateTimeProvider _dateTimeProvider;

    public JwtTokenProvider(IOptions<JwtOptions> options, IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(dateTimeProvider, nameof(dateTimeProvider));

        _jwtOptions = options.Value;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <inheritdoc/>
    public TokenResult IssueToken(Guid userId, string email)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = _dateTimeProvider.UtcNow;
        var expiresAt = now.AddMinutes(_jwtOptions.ExpiryMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(
                JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64
            )
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new TokenResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt
        );
    }
}
