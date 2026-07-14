using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BeTiny.Application.Common.Interfaces.Services;
using BeTiny.Application.Common.Models;
using BeTiny.Application.Common.Options;
using BeTiny.Domain.Interfaces;
using BeTiny.Domain.ValueObjects;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BeTiny.Infrastructure.Services;

/// <inheritdoc/>
public sealed class TokenProvider
    : ITokenProvider
{
    private readonly JwtOptions _jwtOptions;
    private readonly IDateTimeProvider _dateTimeProvider;

    public TokenProvider(IOptions<JwtOptions> options, IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(dateTimeProvider, nameof(dateTimeProvider));

        _jwtOptions = options.Value;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <inheritdoc/>
    public TokenResult IssueToken(UserId userId, Email email)
    {
        ArgumentNullException.ThrowIfNull(userId, nameof(userId));
        ArgumentNullException.ThrowIfNull(email, nameof(email));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.Value.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email.Value),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(
                JwtRegisteredClaimNames.Iat,
                _dateTimeProvider.UtcNow.ToString("o"),
                ClaimValueTypes.DateTime
            )
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: _dateTimeProvider.UtcNow.Add(_jwtOptions.ExpiryMinutes),
            signingCredentials: credentials
        );

        return new TokenResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            _dateTimeProvider.UtcNow.Add(_jwtOptions.ExpiryMinutes)
        );
    }
}
