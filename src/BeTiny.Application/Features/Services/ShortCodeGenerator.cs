using BeTiny.Application.Common.Interfaces.Repositories;
using BeTiny.Application.Common.Interfaces.Services;

namespace BeTiny.Application.Features.Services;

public class ShortCodeGenerator : IShortCodeGenerator
{
    private const string Base62Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    private readonly IKVStore _kVStore;

    #if DEBUG
    static ShortCodeGenerator()
    {
        if (
            Base62Chars.Length != 62 ||
            Base62Chars.Distinct().Count() != 62 ||
            Base62Chars.Any(c => !char.IsAscii(c))
        )
            throw new InvalidOperationException(
                "Base62Chars must contain exactly 62 unique ASCII characters."
            );
    }
    #endif

    public ShortCodeGenerator(IKVStore kVStore)
    {
        _kVStore = kVStore;
    }

    public async Task<string> GenerateShortCode(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var hashSeed = await _kVStore.GetNextHashSeed();

        if (hashSeed < 0)
            throw new InvalidOperationException(
                "Hash seed must be a non-negative number."
            );

        if (hashSeed > 3521614606207L) // 62^7 - 1
            throw new InvalidOperationException(
                "Hash seed exceeds maximum value for 7-character base-62 encoding."
            );

        var buffer = new char[7];
        var pos = buffer.Length;

        do
        {
            buffer[--pos] = Base62Chars[(int)(hashSeed % 62)];
            hashSeed /= 62;
        }
        while (hashSeed > 0);

        return new string(buffer, pos, buffer.Length - pos);
    }
}
