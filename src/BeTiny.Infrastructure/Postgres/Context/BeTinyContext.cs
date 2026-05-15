using Microsoft.EntityFrameworkCore;

namespace BeTiny.Infrastructure.Postgres.Context
{
    public class BeTinyContext : DbContext
    {
        public BeTinyContext(DbContextOptions<BeTinyContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply your entity configurations here
            // Example:
            // modelBuilder.ApplyConfiguration(new YourEntityMapping());
        }
    }
}