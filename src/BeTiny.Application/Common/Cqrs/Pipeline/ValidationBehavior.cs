using BeTiny.Application.Common.Enums;
using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Interfaces.Cqrs.Pipeline;
using BeTiny.Application.Common.Models;
using FluentValidation;

namespace BeTiny.Application.Common.Cqrs.Pipeline;

/// <summary>
/// Validates the request before passing it to the next handler in the pipeline.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationBehavior(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Handles the validation of a request before passing it to the next handler in the pipeline.
    /// </summary>
    /// <param name="request">The request being validated.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The response from the next delegate in the pipeline.</returns>
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct = default
    )
    {
        var requestType = request.GetType();
        var validatorType = typeof(IValidator<>).MakeGenericType(requestType);
        var validator = _serviceProvider.GetService(validatorType) as IValidator;

        if (validator != null)
        {
            var validationResult = validator.Validate(
                new ValidationContext<object>(request)
            );

            if (!validationResult.IsValid)
            {
                var responseType = typeof(TResponse);
                var errorMessage = "Validation failed: " + string.Join(
                    ", ",
                    validationResult.Errors.Select(e => e.ErrorMessage)
                );

                var innerType = responseType.GetGenericArguments()[0];
                var failureMethod = typeof(Result<>)
                    .MakeGenericType(innerType)
                    .GetMethod(
                        nameof(Result<object>.Failure),
                        new[] { typeof(Errors), typeof(string) }
                    )!;

                var result = failureMethod.Invoke(null, new object?[] { Errors.Validation, errorMessage });
                return Task.FromResult((TResponse)result!);
            }
        }

        return next();
    }
}
