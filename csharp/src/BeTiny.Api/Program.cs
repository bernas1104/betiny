using BeTiny.Api.Application;
using BeTiny.Api.Application.Features.ShortenUrl;
using BeTiny.Api.Application.Features.UrlRedirect;
using BeTiny.Api.Domain.Interfaces;
using BeTiny.Api.Infra;
using BeTiny.Api.Infra.Database.Context;

using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddCors();

builder.Services.AddControllers();

builder.Services
    .ConfigureApplication()
    .ConfigureInfra(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(
    opt => opt.AllowAnyHeader()
        .AllowAnyOrigin()
        .AllowAnyMethod()
);

app.MapPost(
    "/v1/api/shorten",
    async (
        [FromServices] ICommandHandler<ShortenUrlRequest, ShortenUrlResponse> handler,
        [FromServices] BeTinyDbContext context,
        [FromBody] ShortenUrlRequest request,
        CancellationToken cancellationToken
    ) =>
    {
        var shortenedUrl = await handler.Handle(request, cancellationToken);

        return Results.CreatedAtRoute(
            "GetRedirectUrl",
            new { shortUrl = shortenedUrl.UrlHash },
            shortenedUrl
        );
    }
    )
    .WithName("PostShortenURL")
    .WithSummary("Takes a long URL and shortens it")
    .WithDescription(
        @"Takes a long URL and generates a hash based on that URL.

The generated hash represents the shorten URL, which will be persisted on the
database.

After shortening the URL, requests made to the GetShortenUrl will map the
to the long URL and performe a redirect.
"
    )
    .WithTags("URL")
    .Produces(StatusCodes.Status201Created)
    .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
    .WithOpenApi();

app.MapGet(
    "/v1/api/{shortUrl}",
    async (
        [FromServices] IQueryHandler<RedirectUrlRequest, RedirectUrlResponse> handler,
        [FromRoute] string shortUrl,
        CancellationToken cancellationToken
    ) =>
    {
        var request = new RedirectUrlRequest(shortUrl);
        var response = await handler.Handle(request, cancellationToken);

        return Results.Redirect(response.LongUrl);
    })
    .WithName("GetRedirectUrl")
    .WithSummary("Takes a short URL and redirects to the respective long URL")
    .WithDescription(
        @"Takes a short URL (hash), maps it to the correct long URL

If the long URL is cached, then the database will not be hit.

If the long URL is not cached, it will be searched on the database.

If no corresponding URL is found, a Not Found (404) will be returned. If it is
found, the result will be cached and the client will ne redirect accordingly.
"
    )
    .WithTags("URL")
    .Produces(StatusCodes.Status302Found)
    .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
    .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
    .WithOpenApi();

app.MapControllers();

app.Run();
