using BeTiny.Domain.Entities;
using BeTiny.Infrastructure.Postgres.Context;
using BeTiny.IntegrationTests.Fixtures;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BeTiny.IntegrationTests.Persistence;

[Collection("IntegrationTests")]
public class PostgresConnectionTest : BaseIntegrationTest, IClassFixture<IntegrationTestFixture>
{
    public PostgresConnectionTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BeTinyContext>();
        await context.Database.MigrateAsync();
    }

    [Fact]
    public async Task CanConnectToDatabase()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BeTinyContext>();

        var canConnect = await context.Database.CanConnectAsync();

        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task CanCreateAndQueryEntity()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BeTinyContext>();

        var shortCode = new Faker().Random.String2(
            7,
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
        );
        var shortUrl = new ShortUrl("https://example.com");
        shortUrl.SetShortCode(shortCode);
        
        await context.ShortUrls.AddAsync(shortUrl);
        await context.SaveChangesAsync();

        var retrieved = await context.ShortUrls
            .FirstOrDefaultAsync(s => s.ShortCode == shortCode);

        retrieved.Should().NotBeNull();
        retrieved!.OriginalUrl.Should().Be("https://example.com");
        retrieved.ShortCode.Should().Be(shortCode);
    }
}
