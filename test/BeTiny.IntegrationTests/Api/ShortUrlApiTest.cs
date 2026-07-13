using System.Net;
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
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await Client.PostAsync("/api/v1/UrlShortener", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CreateShortUrlResponse>(
            responseBody,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );

        result.Should().NotBeNull();
        result!.ShortUrl.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateShortUrl_WithCustomAlias_ReturnsCreated()
    {
        var request = new CreateShortUrlRequest("https://www.example.com", "foo");
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await Client.PostAsync("/api/v1/UrlShortener", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CreateShortUrlResponse>(
            responseBody,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );

        result.Should().NotBeNull();
        result!.ShortUrl.Should().Be("foo");
    }

    [Fact]
    public async Task CreateShortUrl_WithDuplicateShortCode_ReturnsCreated()
    {
        var request = new CreateShortUrlRequest("https://www.example.com");
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var redis = Factory.Services.GetRequiredService<IConnectionMultiplexer>();
        redis.GetDatabase().StringSet("UrlShortener:Counter", 0);
        
        await Client.PostAsync("/api/v1/UrlShortener", content);
        var response = await Client.PostAsync("/api/v1/UrlShortener", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CreateShortUrlResponse>(
            responseBody,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );

        result.Should().NotBeNull();
        result!.ShortUrl.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateShortUrl_WithDuplicateCustomAlias_ReturnsConflict()
    {
        var request = new CreateShortUrlRequest("https://www.example.com", "customalias123");
        var duplicateRequest = new CreateShortUrlRequest("https://www.example.com", "customalias123");
        
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        await Client.PostAsync("/api/v1/UrlShortener", content);

        json = JsonSerializer.Serialize(duplicateRequest);
        content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await Client.PostAsync("/api/v1/UrlShortener", content);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ProblemDetails>(
            responseBody,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );

        result.Should().NotBeNull();
        result!.Title.Should().Be("ConflictError");
    }

    [Fact]
    public async Task ShortenUrl_WithReservedCustomAlias_ReturnsBadRequest()
    {
        var request = new CreateShortUrlRequest("https://www.example.com", "admin");
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await Client.PostAsync("/api/v1/UrlShortener", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ProblemDetails>(
            responseBody,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );

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
