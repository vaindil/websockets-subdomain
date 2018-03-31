using Microsoft.EntityFrameworkCore;
using WebSockets.Data.Configs;

namespace WebSockets.Data
{
    public class VbContext : DbContext
    {
        public VbContext(DbContextOptions<VbContext> options)
            : base(options)
        {
        }

        public DbSet<KeyValue> KeyValues { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new KeyValueConfiguration());
        }
    }
}
