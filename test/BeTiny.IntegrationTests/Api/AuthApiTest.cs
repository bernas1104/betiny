using System.Net;
using System.Text;
using System.Text.Json;
using BeTiny.Application.Features.Auth.Commands.Register;
using BeTiny.Infrastructure.Postgres.Context;
using BeTiny.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BeTiny.IntegrationTests.Api;

[Collection("IntegrationTests")]
public class AuthApiTest : BaseIntegrationTest, IClassFixture<IntegrationTestFixture>
{
    public AuthApiTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BeTinyContext>();
        await context.Database.MigrateAsync();
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Users\" RESTART IDENTITY CASCADE");
    }

    [Fact]
    public async Task Register_ReturnsCreated_WhenValidInput()
    {
        var request = new RegisterRequest("user@example.com", "Password1");
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await Client.PostAsync("/api/v1/auth/register", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<RegisterResponse>(
            responseBody,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );

        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result.Email.Should().Be("user@example.com");
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenEmailInvalid()
    {
        var request = new RegisterRequest("notanemail", "Password1");
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await Client.PostAsync("/api/v1/auth/register", content);

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
    public async Task Register_ReturnsBadRequest_WhenPasswordWeak()
    {
        var request = new RegisterRequest("user@example.com", "short");
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await Client.PostAsync("/api/v1/auth/register", content);

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
    public async Task Register_ReturnsConflict_WhenEmailDuplicate()
    {
        var request = new RegisterRequest("user@example.com", "Password1");
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        await Client.PostAsync("/api/v1/auth/register", content);

        var duplicateRequest = new RegisterRequest("user@example.com", "Password1");
        json = JsonSerializer.Serialize(duplicateRequest);
        content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await Client.PostAsync("/api/v1/auth/register", content);

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
    public async Task Register_NormalizesEmailBeforeUniqueness()
    {
        var firstRequest = new RegisterRequest("User@Example.COM", "Password1");
        var json = JsonSerializer.Serialize(firstRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        await Client.PostAsync("/api/v1/auth/register", content);

        var secondRequest = new RegisterRequest("user@example.com", "Password1");
        json = JsonSerializer.Serialize(secondRequest);
        content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await Client.PostAsync("/api/v1/auth/register", content);

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
    public async Task Register_PersistsHashedPassword_NotPlaintext()
    {
        var request = new RegisterRequest("user@example.com", "Password1");
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        await Client.PostAsync("/api/v1/auth/register", content);

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BeTinyContext>();
        var user = await context.Users.FirstAsync();

        user.PasswordHash.Should().NotBe("Password1");
        user.PasswordHash.Should().StartWith("$2");
    }
}
