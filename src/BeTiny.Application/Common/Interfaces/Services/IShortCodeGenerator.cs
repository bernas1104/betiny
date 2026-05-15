namespace BeTiny.Application.Common.Interfaces.Services;

public interface IShortCodeGenerator
{
    /// <summary>
    /// Generates a unique short code for URL shortening.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A base-62 encoded short code string</returns>
    Task<string> GenerateShortCode(CancellationToken ct = default);
}
