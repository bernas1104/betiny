namespace BeTiny.Application.Features.Auth.Commands.Login;

public sealed record LoginResponse(string Token, DateTime ExpiresAt);
