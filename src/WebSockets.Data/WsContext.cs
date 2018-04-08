using Microsoft.EntityFrameworkCore;
using WebSockets.Data.Configs;
using WebSockets.Data.Internal;
using WebSockets.Data.Twitch;

namespace WebSockets.Data
{
    public class WsContext : DbContext
    {
        public WsContext(DbContextOptions<WsContext> options)
            : base(options)
        {
        }

        public DbSet<KeyValue> KeyValues { get; set; }
        public DbSet<TwitchStream> TwitchStreams { get; set; }
        public DbSet<TwitchMarker> TwitchMarkers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new KeyValueConfiguration());
            modelBuilder.ApplyConfiguration(new TwitchStreamConfiguration());
            modelBuilder.ApplyConfiguration(new TwitchMarkerConfiguration());
        }
    }
}
