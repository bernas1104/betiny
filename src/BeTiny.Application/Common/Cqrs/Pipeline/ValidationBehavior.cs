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
public class ValidationBehavior<TRequest, TResponse>(IServiceProvider serviceProvider)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult, new()
{
    /// <summary>
    /// Handles the validation of a request before passing it to the next handler in the pipeline.
    /// </summary>
    /// <param name="request">The request being validated.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The response from the next delegate in the pipeline.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct = default
    )
    {
        var requestType = request.GetType();
        var validatorType = typeof(IValidator<>).MakeGenericType(requestType);
        var validator = serviceProvider.GetService(validatorType) as IValidator;

        if (validator != null)
        {
            var validationResult = await validator.ValidateAsync(
                new ValidationContext<object>(request),
                ct
            );

            if (!validationResult.IsValid)
            {
                var errorMessage = "Validation failed: " + string.Join(
                    ", ",
                    validationResult.Errors.Select(e => e.ErrorMessage)
                );

                return new TResponse
                {
                    Errors = [..validationResult.Errors.Select(
                        e => new Error(
                            ErrorTypes.ValidationError,
                            e.PropertyName,
                            e.ErrorMessage,
                            ErrorSeverity.Low
                        )
                    )]
                };
            }
        }

        return await next(ct);
    }
}
