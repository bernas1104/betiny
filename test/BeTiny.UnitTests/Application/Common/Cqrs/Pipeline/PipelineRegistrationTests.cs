using BeTiny.Application.Common.Cqrs.Pipeline;
using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Interfaces.Cqrs.Pipeline;
using BeTiny.Application.Common.Models;
using BeTiny.Application.Features.UrlShortening.Commands.CreateShortUrl;
using BeTiny.IOC.DependencyInjections;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BeTiny.UnitTests.Application.Common.Cqrs.Pipeline;

public class PipelineRegistrationTests
{
    [Fact]
    public void PipelineBehaviors_Are_Resolvable_For_CreateShortUrlRequest()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddValidatorsFromAssemblyContaining<CreateShortUrlRequestValidator>();
        services.RegisterHandlers();

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var behaviorInterface = typeof(IPipelineBehavior<,>)
            .MakeGenericType(
                typeof(IRequest<Result<CreateShortUrlResponse>>),
                typeof(Result<CreateShortUrlResponse>)
            );

        var enumerableType = typeof(IEnumerable<>).MakeGenericType(behaviorInterface);
        var behaviors = ((System.Collections.IEnumerable)scope.ServiceProvider.GetRequiredService(enumerableType))
            .Cast<object>();

        var behaviorTypes = behaviors.Select(b => b.GetType()).ToList();

        behaviors.Should().NotBeEmpty();
        behaviorTypes.Should().Contain(t => t == typeof(ValidationBehavior<,>).MakeGenericType(
            typeof(IRequest<Result<CreateShortUrlResponse>>),
            typeof(Result<CreateShortUrlResponse>)));
        behaviorTypes.Should().Contain(t => t == typeof(LoggingBehavior<,>).MakeGenericType(
            typeof(IRequest<Result<CreateShortUrlResponse>>),
            typeof(Result<CreateShortUrlResponse>)));
    }
}
