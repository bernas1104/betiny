using BeTiny.Application.Common.Interfaces.Cqrs.Contracts;
using BeTiny.Application.Common.Models;

namespace BeTiny.Application.Features.Auth.Queries.GetMe;

public sealed record GetMeRequest : IQuery<Result<GetMeResponse>>;
