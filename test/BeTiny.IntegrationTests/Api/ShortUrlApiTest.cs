using System.Net;
using System.Text;
using System.Text.Json;
using BeTiny.Application.Common.Enums;
using BeTiny.Application.Common.Interfaces.Cqrs;
using BeTiny.Application.Features.UrlShortening.Commands.CreateShortUrl;
using BeTiny.Application.Features.UrlShortening.Queries.GetByShortCode;
using BeTiny.Infrastructure.Postgres.Context;
using BeTiny.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
    public async Task GetByShortCode_ReturnsOriginalUrl_WhenShortCodeExists()
    {
        var createRequest = new CreateShortUrlRequest("https://www.example.com");
        var json = JsonSerializer.Serialize(createRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var createResponse = await Client.PostAsync("/api/v1/UrlShortener", content);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseBody = await createResponse.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CreateShortUrlResponse>(
            responseBody,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );
        result.Should().NotBeNull();
        var shortCode = result!.ShortUrl;

        using var scope = Factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var queryResult = await sender.Send(
            new GetByShortCodeRequest(
                shortCode,
                "TestAgent",
                "http://test.com",
                "127.0.0.1"
            )
        );

        queryResult.IsSuccess.Should().BeTrue();
        queryResult.Value.Should().NotBeNull();
        queryResult.Value!.OriginalUrl.Should().Be("https://www.example.com");
    }

    [Fact]
    public async Task GetByShortCode_ReturnsNotFound_WhenShortCodeDoesNotExist()
    {
        using var scope = Factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var queryResult = await sender.Send(
            new GetByShortCodeRequest(
                "notfnd",
                "TestAgent",
                "http://test.com",
                "127.0.0.1"
            )
        );

        queryResult.IsSuccess.Should().BeFalse();
        queryResult.Errors.Should().NotBeNullOrEmpty();
        queryResult.Errors!.First().ErrorType.Should().Be(ErrorTypes.NotFoundError);
    }
}
