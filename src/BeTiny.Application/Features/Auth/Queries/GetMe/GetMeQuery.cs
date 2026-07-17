using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Models;
using BeTiny.Domain.Common.Interfaces;
using BeTiny.Domain.Enums;

namespace BeTiny.Application.Features.Auth.Queries.GetMe;

public sealed class GetMeQuery(ICurrentUser currentUser)
    : IRequestHandler<GetMeRequest, Result<GetMeResponse>>
{
    public Task<Result<GetMeResponse>> Handle(
        GetMeRequest request,
        CancellationToken cancellationToken
    )
    {
        if (currentUser.UserId is null) 
            throw new InvalidOperationException("Authenticated user ID not found in claims.");

        return Task.FromResult(
            Result<GetMeResponse>.Success(
                new GetMeResponse(
                    currentUser.UserId.Value,
                    currentUser.Email ?? string.Empty,
                    (currentUser.Plan ?? Plans.Free).ToString()
                )
            )
        );
    }
}
