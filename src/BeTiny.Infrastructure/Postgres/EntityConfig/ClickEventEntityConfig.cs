using BeTiny.Domain.Entities;
using BeTiny.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeTiny.Infrastructure.Postgres.EntityConfig;

/// <summary>
/// Entity Framework Core configuration for the <see cref="ClickEvent"/> entity.
/// </summary>
public class ClickEventEntityConfig : IEntityTypeConfiguration<ClickEvent>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ClickEvent> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(
                id => id.Value,
                value => ClickEventId.Create(value)
            );

        builder.Property(e => e.ShortUrlId)
            .HasConversion(
                id => id.Value,
                value => ShortUrlId.Create(value)
            );

        builder.Property(e => e.IpAddress).HasMaxLength(11);
        builder.Property(e => e.Country).HasMaxLength(100);
        builder.Property(e => e.UserAgent).HasMaxLength(500);
        builder.Property(e => e.Referer).HasMaxLength(500);
        builder.Property(e => e.DeviceType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasOne(e => e.ShortUrl)
            .WithMany(s => s.ClickEvents)
            .HasForeignKey(e => e.ShortUrlId);

        builder.HasIndex(x => x.CreatedAt)
            .IsDescending();
    }
}
