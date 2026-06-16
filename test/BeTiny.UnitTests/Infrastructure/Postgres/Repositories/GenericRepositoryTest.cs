using BeTiny.Domain.Entities;
using BeTiny.Domain.Enums;
using BeTiny.Domain.ValueObjects;
using BeTiny.Infrastructure.Postgres.Context;
using BeTiny.Infrastructure.Postgres.Repositories;
using BeTiny.UnitTests.Fixtures;
using Bogus;

namespace BeTiny.UnitTests.Infrastructure.Postgres.Repositories;

public sealed class GenericRepositoryTest
{
    private readonly BeTinyContext _context;
    private readonly GenericRepository<ShortUrl, ShortUrlId, Guid> _repository;
    private readonly Faker _faker = new ();

    public GenericRepositoryTest()
    {
        _context = GenericRepositoryTestFixture.CreateInMemoryContext();
        _repository = new GenericRepository<ShortUrl, ShortUrlId, Guid>(_context);
    }

    [Fact]
    public async Task GetByFilterAsync_WhenEntityMatchesFilter_ReturnsEntity()
    {
        var shortUrl = new ShortUrl("https://example.com", _faker.PickRandom<AliasUrlType>());
        shortUrl.SetAliasUrl("foo");

        _context.ShortUrls.Add(shortUrl);
        _context.SaveChanges();

        var result = await _repository.GetByFilterAsync(e => e.AliasUrl == "foo");

        result.Should().NotBeNull();
        result!.AliasUrl.Should().Be("foo");
    }

    [Fact]
    public async Task GetByFilterAsync_WhenNoEntityMatchesFilter_ReturnsNull()
    {
        var result = await _repository.GetByFilterAsync(e => e.AliasUrl == "nonexistent");

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_WhenCalled_AddsEntityToContext()
    {
        var shortUrl = new ShortUrl("https://example.com", _faker.PickRandom<AliasUrlType>());
        shortUrl.SetAliasUrl("bar");

        await _repository.AddAsync(shortUrl);

        var result = _context.ShortUrls.FirstOrDefault(e => e.AliasUrl == "bar");

        result.Should().NotBeNull();
        result!.AliasUrl.Should().Be("bar");
    }
}
