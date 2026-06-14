using System.Diagnostics.CodeAnalysis;
using BeTiny.Domain.Common.Entities;
using BeTiny.Domain.Enums;
using BeTiny.Domain.ValueObjects;

namespace BeTiny.Domain.Entities;

[ExcludeFromCodeCoverage]
public sealed class ClickEvent : AggregateRoot<ClickEventId, Guid>
{
    public ShortUrlId ShortUrlId { get; private set; }
    public ShortUrl ShortUrl { get; set; } = null!;
    public string IpAddress { get; private set; }
    public string Country { get; private set; }
    public string UserAgent { get; private set; }
    public string Referer { get; private set; }
    public DeviceTypes DeviceType { get; private set; }

    #pragma warning disable CS8618
    // Private empty constructor needed by EF Core
    private ClickEvent()
    {
    }
    #pragma warning restore

    public ClickEvent(
        ShortUrlId shortUrlId,
        string ipAddress,
        string country,
        string userAgent,
        string referer,
        DeviceTypes deviceType
    )
    {
        Id = ClickEventId.CreateUnique();
        ShortUrlId = shortUrlId;
        IpAddress = ipAddress;
        Country = country;
        UserAgent = userAgent;
        Referer = referer;
        DeviceType = deviceType;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }
}
