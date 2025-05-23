using BeTiny.Api.Domain.Entites;
using BeTiny.Api.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeTiny.Api.Infra.Database.Mappings
{
    public class UrlMapping : IEntityTypeConfiguration<Url>
    {
        public void Configure(EntityTypeBuilder<Url> builder)
        {
            builder.ToTable("Urls");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .ValueGeneratedNever()
                .HasConversion(
                    id => id.Value,
                    value => UrlId.Create(value)
                );

            builder.Property(x => x.LongUrl)
                .HasMaxLength(100)
                .HasColumnName("LongUrl")
                .IsRequired();

            builder.Property(x => x.Accesses)
                .HasColumnName("Accesses")
                .HasConversion(
                    counter => counter.Value,
                    value => new Counter(value)
                )
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired();

            builder.HasIndex(x => x.LongUrl)
                .IsUnique();
        }
    }
}
