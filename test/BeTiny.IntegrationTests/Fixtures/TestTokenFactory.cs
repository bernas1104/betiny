using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BeTiny.Domain.Enums;
using Microsoft.IdentityModel.Tokens;

namespace BeTiny.IntegrationTests.Fixtures;

public sealed class TestTokenFactory(InfrastructureFixture infrastructureFixture)
{
    private const string InvalidSigningKey = "Z1nd3xInvalidSignatureKeyForTests1234567890=";
    
    public string CreateToken(
        Guid userId,
        string email,
        string plan = "Free",
        DateTime? expiresAt = null
    ) => BuildToken(userId, email, plan, expiresAt, infrastructureFixture.JwtSigningKey);

    public string CreateExpiredToken(
        Guid userId,
        string email,
        string plan = "Free",
        TimeSpan? expiryOffset = null
    )
    {
        var offset = expiryOffset ?? TimeSpan.FromMinutes(10);
        return BuildToken(userId, email, plan, DateTime.UtcNow.Subtract(offset), infrastructureFixture.JwtSigningKey);
    }

    public string CreateInvalidSignatureToken(
        Guid userId,
        string email,
        string plan = "Free"
    ) => BuildToken(userId, email, plan, null, InvalidSigningKey);

    private string BuildToken(
        Guid userId,
        string email,
        string plan,
        DateTime? expiresAt,
        string signingKey
    )
    {
        var now = expiresAt is null
            ? DateTime.UtcNow
            : expiresAt.Value.Subtract(TimeSpan.FromMinutes(infrastructureFixture.JwtExpiryMinutes));

        var expires = expiresAt ?? now.AddMinutes(infrastructureFixture.JwtExpiryMinutes);

        var claims = new []
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim("plan", plan),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(
                JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(now).ToUnixTimeSeconds().ToString()
            )
        };

        var token = new JwtSecurityToken(
            issuer: infrastructureFixture.JwtIssuer,
            audience: infrastructureFixture.JwtAudience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                SecurityAlgorithms.HmacSha256
            )
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
