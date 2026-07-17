namespace BeTiny.Application.Features.Auth.Queries.GetMe;

public sealed record GetMeResponse(Guid Id, string Email, string Plan);
