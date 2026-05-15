namespace BeTiny.Application.Common.Interfaces.Services;

public interface IShortCodeGenerator
{
    /// <inheritdoc/>
    Task<string> GenerateShortCode(CancellationToken ct = default);
}
