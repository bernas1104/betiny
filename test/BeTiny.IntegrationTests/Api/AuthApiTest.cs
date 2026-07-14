using System.Net;
using System.Net.Http.Json;
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
    public async Task Register_ValidInput_ReturnsCreatedAsync()
    {
        var request = new RegisterRequest("user@example.com", "Password1");
        
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();

        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result.Email.Should().Be("user@example.com");
    }

    [Fact]
    public async Task Register_EmailInvalid_ReturnsBadRequestAsync()
    {
        var request = new RegisterRequest("notanemail", "Password1");
        
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        result.Should().NotBeNull();
        result!.Title.Should().Be("ValidationError");
    }

    [Fact]
    public async Task Register_PasswordWeak_ReturnsBadRequestAsync()
    {
        var request = new RegisterRequest("user@example.com", "short");
        
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        result.Should().NotBeNull();
        result!.Title.Should().Be("ValidationError");
    }

    [Fact]
    public async Task Register_EmailDuplicate_ReturnsConflictAsync()
    {
        var request = new RegisterRequest("user@example.com", "Password1");
        await Client.PostAsJsonAsync("/api/v1/auth/register", request);

        var duplicateRequest = new RegisterRequest("user@example.com", "Password1");
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", duplicateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        result.Should().NotBeNull();
        result!.Title.Should().Be("ConflictError");
    }

    [Fact]
    public async Task Register_EmailsDifferByCase_ReturnsConflictAsync()
    {
        var firstRequest = new RegisterRequest("User@Example.COM", "Password1");
        await Client.PostAsJsonAsync("/api/v1/auth/register", firstRequest);

        var secondRequest = new RegisterRequest("user@example.com", "Password1");

        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", secondRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        result.Should().NotBeNull();
        result!.Title.Should().Be("ConflictError");
    }

    [Fact]
    public async Task Register_ValidInput_PersistsHashedPasswordAsync()
    {
        var request = new RegisterRequest("user@example.com", "Password1");
        await Client.PostAsJsonAsync("/api/v1/auth/register", request);

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BeTinyContext>();
        var user = await context.Users.FirstAsync();

        user.PasswordHash.Should().NotBe("Password1");
        user.PasswordHash.Should().StartWith("$2");
    }
}
