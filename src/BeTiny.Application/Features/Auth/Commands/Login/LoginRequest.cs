using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Models;

namespace BeTiny.Application.Features.Auth.Commands.Login;

public sealed record LoginRequest(string Email, string Password) : ICommand<Result<LoginResponse>>;
