using BeTiny.Domain.Common.Interfaces;
using BeTiny.Domain.Enums;
using BeTiny.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace BeTiny.Infrastructure.Services;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public UserId? UserId { get => RetrieveUserId(httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value); }

    public string? Email { get => httpContextAccessor.HttpContext?.User.FindFirst("email")?.Value; }

    public Plans? Plan { get => RetrievePlan(httpContextAccessor.HttpContext?.User.FindFirst("plan")?.Value); }

    public bool IsAuthenticated { get => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false; }

    private static UserId? RetrieveUserId(string? sub) =>
        sub is not null && Guid.TryParse(sub, out var userId)
            ? UserId.Create(userId)
            : null;

    private static Plans? RetrievePlan(string? plan) =>
        plan is not null && Enum.TryParse<Plans>(plan, out var parsedPlan)
            ? parsedPlan
            : null;
}
