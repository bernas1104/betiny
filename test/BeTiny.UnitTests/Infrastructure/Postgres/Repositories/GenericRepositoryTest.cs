using BeTiny.Domain.Entities;
using BeTiny.Domain.ValueObjects;
using BeTiny.Infrastructure.Postgres.Context;
using BeTiny.Infrastructure.Postgres.Repositories;
using BeTiny.UnitTests.Fixtures;

namespace BeTiny.UnitTests.Infrastructure.Postgres.Repositories;

public sealed class GenericRepositoryTest
{
    private readonly BeTinyContext _context;
    private readonly GenericRepository<ShortUrl, ShortUrlId, Guid> _repository;

    public GenericRepositoryTest()
    {
        _context = GenericRepositoryTestFixture.CreateInMemoryContext();
        _repository = new GenericRepository<ShortUrl, ShortUrlId, Guid>(_context);
    }

    [Fact]
    public async Task GetByFilterAsync_ShouldReturnEntity_WhenEntityMatchesFilter()
    {
        var shortUrl = new ShortUrl("https://example.com", "foo");

        _context.ShortUrls.Add(shortUrl);
        await _repository.SaveChanges();

        var result = await _repository.GetByFilterAsync(e => e.ShortCode == "foo");

        Assert.NotNull(result);
        Assert.Equal("foo", result!.ShortCode);
    }

    [Fact]
    public async Task GetByFilterAsync_ShouldReturnNull_WhenNoEntityMatchesFilter()
    {
        var result = await _repository.GetByFilterAsync(e => e.ShortCode == "nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntityToContext()
    {
        var shortUrl = new ShortUrl("https://example.com", "bar");

        await _repository.AddAsync(shortUrl);
        await _repository.SaveChanges();

        var result = _context.ShortUrls.FirstOrDefault(e => e.ShortCode == "bar");

        Assert.NotNull(result);
        Assert.Equal("bar", result!.ShortCode);
    }
}
