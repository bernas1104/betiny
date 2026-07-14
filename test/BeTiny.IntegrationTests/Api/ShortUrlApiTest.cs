using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BeTiny.Application.Common.Interfaces.Cqrs;
using BeTiny.Application.Features.UrlShortening.Commands.CreateShortUrl;
using BeTiny.Infrastructure.Postgres.Context;
using BeTiny.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace BeTiny.IntegrationTests.Api;

[Collection("IntegrationTests")]
public class ShortUrlApiTest : BaseIntegrationTest, IClassFixture<IntegrationTestFixture>
{
    public ShortUrlApiTest(IntegrationTestFixture fixture) : base(fixture)
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
    public async Task CreateShortUrl_ReturnsCreated()
    {
        var request = new CreateShortUrlRequest("https://www.example.com");
        
        var response = await Client.PostAsJsonAsync("/api/v1/UrlShortener", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateShortUrlResponse>();

        result.Should().NotBeNull();
        result!.ShortUrl.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateShortUrl_WithCustomAlias_ReturnsCreated()
    {
        var request = new CreateShortUrlRequest("https://www.example.com", "foo");
        var response = await Client.PostAsJsonAsync("/api/v1/UrlShortener", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateShortUrlResponse>();

        result.Should().NotBeNull();
        result!.ShortUrl.Should().Be("foo");
    }

    [Fact]
    public async Task CreateShortUrl_WithDuplicateShortCode_ReturnsCreated()
    {
        var request = new CreateShortUrlRequest("https://www.example.com");

        var redis = Factory.Services.GetRequiredService<IConnectionMultiplexer>();
        redis.GetDatabase().StringSet("UrlShortener:Counter", 0);
        
        await Client.PostAsJsonAsync("/api/v1/UrlShortener", request);
        var response = await Client.PostAsJsonAsync("/api/v1/UrlShortener", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateShortUrlResponse>();

        result.Should().NotBeNull();
        result!.ShortUrl.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateShortUrl_WithDuplicateCustomAlias_ReturnsConflict()
    {
        var request = new CreateShortUrlRequest("https://www.example.com", "customalias123");
        var duplicateRequest = new CreateShortUrlRequest("https://www.example.com", "customalias123");
        
        await Client.PostAsJsonAsync("/api/v1/UrlShortener", request);
        
        var response = await Client.PostAsJsonAsync("/api/v1/UrlShortener", duplicateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        result.Should().NotBeNull();
        result!.Title.Should().Be("ConflictError");
    }

    [Fact]
    public async Task CreateShortUrl_WithReservedCustomAlias_ReturnsBadRequest()
    {
        var request = new CreateShortUrlRequest("https://www.example.com", "admin");

        var response = await Client.PostAsJsonAsync("/api/v1/UrlShortener", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        result.Should().NotBeNull();
        result!.Title.Should().Be("ValidationError");
    }

    [Fact]
    public async Task GetByShortUrl_ReturnsOriginalUrl_WhenShortUrlExists()
    {
        using var scope = Factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        
        var commandResult = await sender.Send(new CreateShortUrlRequest("https://www.example.com"));
        commandResult.IsSuccess.Should().BeTrue();

        var shortUrl = commandResult.Value!.ShortUrl!;

        var response = await Client.GetAsync($"/api/v1/UrlShortener/{shortUrl}");
        
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task GetByShortUrl_ReturnsNotFound_WhenShortUrlDoesNotExist()
    {
        var shortUrl = "notfound";
        var response = await Client.GetAsync($"/api/v1/UrlShortener/{shortUrl}");
        
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
