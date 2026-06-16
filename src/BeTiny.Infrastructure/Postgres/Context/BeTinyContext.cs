using BeTiny.Domain.Entities;
using BeTiny.Infrastructure.Postgres.EntityConfig;
using Microsoft.EntityFrameworkCore;

namespace BeTiny.Infrastructure.Postgres.Context
{
    /// <summary>
    /// The Entity Framework Core database context for the BeTiny application.
    /// </summary>
    public class BeTinyContext : DbContext
    {
        /// <summary>
        /// Gets or sets the Users table.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the ShortUrls table.
        /// </summary>
        public DbSet<ShortUrl> ShortUrls { get; set; }

        /// <summary>
        /// Gets or sets the ClickEvents table.
        /// </summary>
        public DbSet<ClickEvent> ClickEvents { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeTinyContext"/> class.
        /// </summary>
        /// <param name="options">The database context options.</param>
        public BeTinyContext(DbContextOptions<BeTinyContext> options)
            : base(options)
        {
        }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new UserEntityConfig());
            modelBuilder.ApplyConfiguration(new ShortUrlEntityConfig());
            modelBuilder.ApplyConfiguration(new ClickEventEntityConfig());
        }
    }
}