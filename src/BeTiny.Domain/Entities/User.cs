using BeTiny.Domain.Common.Entities;
using BeTiny.Domain.Enums;
using BeTiny.Domain.ValueObjects;

namespace BeTiny.Domain.Entities;

public sealed class User : AggregateRoot<UserId, Guid>
{
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public Plans Plan { get; private set; }

    #pragma warning disable CS8618
    // Private empty constructor needed by EF Core
    private User()
    {
    }
    #pragma warning restore

    public User(string email)
    {
        Id = UserId.CreateUnique();
        Email = email;
        PasswordHash = string.Empty;
        Plan = Plans.Free;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }
}
