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
    public async Task GetByFilterAsync_WhenEntityMatchesFilter_ShouldReturnEntity()
    {
        var shortUrl = new ShortUrl("https://example.com", "foo");

        _context.ShortUrls.Add(shortUrl);
        await _repository.SaveChanges();

        var result = await _repository.GetByFilterAsync(e => e.ShortCode == "foo");

        result.Should().NotBeNull();
        result!.ShortCode.Should().Be("foo");
    }

    [Fact]
    public async Task GetByFilterAsync_WhenNoEntityMatchesFilter_ShouldReturnNull()
    {
        var result = await _repository.GetByFilterAsync(e => e.ShortCode == "nonexistent");

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_WhenCalled_ShouldAddEntityToContext()
    {
        var shortUrl = new ShortUrl("https://example.com", "bar");

        await _repository.AddAsync(shortUrl);
        await _repository.SaveChanges();

        var result = _context.ShortUrls.FirstOrDefault(e => e.ShortCode == "bar");

        result.Should().NotBeNull();
        result!.ShortCode.Should().Be("bar");
    }
}
