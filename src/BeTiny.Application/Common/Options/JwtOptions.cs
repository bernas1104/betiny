namespace BeTiny.Application.Common.Options;

public sealed class JwtOptions
{
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SigningKey { get; init; } = string.Empty;
    public TimeSpan ExpiryMinutes { get; init; } = TimeSpan.FromMinutes(60);
}
