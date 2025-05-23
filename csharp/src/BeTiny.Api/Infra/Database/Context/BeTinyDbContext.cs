using BeTiny.Api.Domain.Entites;
using BeTiny.Api.Infra.Database.Mappings;

using Microsoft.EntityFrameworkCore;

namespace BeTiny.Api.Infra.Database.Context
{
    public class BeTinyDbContext : DbContext
    {
        public DbSet<Url> Urls { get; set; }

        public BeTinyDbContext(DbContextOptions<BeTinyDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new UrlMapping());
        }
    }
}
