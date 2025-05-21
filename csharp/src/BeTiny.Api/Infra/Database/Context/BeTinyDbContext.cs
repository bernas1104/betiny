using Microsoft.EntityFrameworkCore;

namespace BeTiny.Api.Infra.Database.Context
{
    public class BeTinyDbContext : DbContext
    {
        public BeTinyDbContext(DbContextOptions<BeTinyDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
