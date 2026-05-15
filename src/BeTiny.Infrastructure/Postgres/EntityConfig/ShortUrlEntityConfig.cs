using BeTiny.Domain.Entities;
using BeTiny.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeTiny.Infrastructure.Postgres.EntityConfig;

public class ShortUrlEntityConfig : IEntityTypeConfiguration<ShortUrl>
{
    public void Configure(EntityTypeBuilder<ShortUrl> builder)
    {
        builder.ToTable("ShortUrls");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => ShortUrlId.Create(value)
            );

        builder.Property(x => x.UserId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                value => value == null ? null : UserId.Create(value.Value)
            );

        builder.Property(x => x.OriginalUrl)
            .HasMaxLength(2048)
            .HasColumnName("OriginalUrl")
            .IsRequired();

        builder.Property(x => x.ShortCode)
            .HasMaxLength(7)
            .HasColumnName("ShortCode")
            .IsRequired();

        builder.HasIndex(x => x.ShortCode)
            .IsUnique();

        builder.Property(x => x.CustomAlias)
            .HasMaxLength(50)
            .HasColumnName("CustomAlias");

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("ExpiresAt");
    }
}
