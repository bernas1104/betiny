using BeTiny.Domain.Entities;
using BeTiny.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeTiny.Infrastructure.Postgres.EntityConfig;

/// <summary>
/// Entity Framework Core configuration for the <see cref="ShortUrl"/> entity.
/// </summary>
public class ShortUrlEntityConfig : IEntityTypeConfiguration<ShortUrl>
{
    /// <inheritdoc/>
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

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.Property(x => x.OriginalUrl)
            .HasMaxLength(2048)
            .HasColumnName("OriginalUrl")
            .IsRequired();

        builder.Property(x => x.AliasUrl)
            .HasMaxLength(50)
            .HasColumnName("AliasUrl")
            .IsRequired();

        builder.HasIndex(x => x.AliasUrl)
            .IsUnique();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasColumnName("Type")
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("ExpiresAt");
    }
}
