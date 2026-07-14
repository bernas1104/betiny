using BeTiny.Domain.Entities;
using BeTiny.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BeTiny.Infrastructure.Postgres.EntityConfig;

/// <summary>
/// Entity Framework Core configuration for the <see cref="User"/> entity.
/// </summary>
public class UserEntityConfig : IEntityTypeConfiguration<User>
{
    /// <inheritdoc/>
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => UserId.Create(value)
            );

        builder.Property(x => x.Email)
            .HasConversion(
                e => e.Value,
                v => Email.Create(v)
            )
            .HasMaxLength(254)
            .HasColumnName("Email")
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(255)
            .HasColumnName("PasswordHash")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();
    }
}
