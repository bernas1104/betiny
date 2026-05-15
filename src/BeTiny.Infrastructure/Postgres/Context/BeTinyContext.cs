using BeTiny.Domain.Entities;
using BeTiny.Infrastructure.Postgres.EntityConfig;
using Microsoft.EntityFrameworkCore;

namespace BeTiny.Infrastructure.Postgres.Context
{
    public class BeTinyContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public BeTinyContext(DbContextOptions<BeTinyContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new UserEntityConfig());
        }
    }
}