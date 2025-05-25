using BeTiny.Api.Application;
using BeTiny.Api.Application.Common.Helpers;
using BeTiny.Api.Application.Common.Models;
using BeTiny.Api.Application.Features.Commands.ShortenUrl;
using BeTiny.Api.Application.Features.Queries.UrlRedirect;
using BeTiny.Api.Domain.Interfaces.CQRS;
using BeTiny.Api.Infra;

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
        [FromServices] ICommandHandler<ShortenUrlRequest, Result<ShortenUrlResponse>> handler,
        [FromBody] ShortenUrlRequest request,
        HttpContext context,
        CancellationToken cancellationToken
    ) =>
    {
        var result = await handler.Handle(request, cancellationToken);

        return result.IsSuccess ?
            Results.CreatedAtRoute(
                "GetRedirectUrl",
                new { shortUrl = result.Value?.UrlHash },
                result
            ) :
            ResultsHelper.GetResultFromError(context, result);
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
    .Produces<ShortenUrlResponse>(StatusCodes.Status201Created)
    .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
    .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
    .WithOpenApi();

app.MapGet(
    "/v1/api/{shortUrl}",
    async (
        [FromServices] IQueryHandler<RedirectUrlRequest, Result<RedirectUrlResponse>> handler,
        [FromRoute] string shortUrl,
        HttpContext context,
        CancellationToken cancellationToken
    ) =>
    {
        var request = new RedirectUrlRequest(shortUrl);
        var result = await handler.Handle(request, cancellationToken);

        return result.IsSuccess ?
            Results.Redirect(result.Value?.LongUrl!) :
            ResultsHelper.GetResultFromError(context, result);
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
