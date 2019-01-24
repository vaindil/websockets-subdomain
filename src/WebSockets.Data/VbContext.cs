using Microsoft.EntityFrameworkCore;
using WebSockets.Data.Configs;
using WebSockets.Data.Configs.ZubatRequests;
using WebSockets.Data.ZubatRequests;

namespace WebSockets.Data
{
    public class VbContext : DbContext
    {
        public VbContext(DbContextOptions<VbContext> options)
            : base(options)
        {
        }

        public DbSet<KeyValue> KeyValues { get; set; }
        public DbSet<EmoteVote> EmoteVotes { get; set; }
        public DbSet<RequestUser> RequestUsers { get; set; }
        public DbSet<KillerRequest> KillerRequests { get; set; }
        public DbSet<SurvivorRequest> SurvivorRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new KeyValueConfiguration());
            modelBuilder.ApplyConfiguration(new EmoteVoteConfiguration());
            modelBuilder.ApplyConfiguration(new RequestUserConfiguration());
            modelBuilder.ApplyConfiguration(new KillerRequestConfiguration());
            modelBuilder.ApplyConfiguration(new SurvivorRequestConfiguration());
        }
    }
}
