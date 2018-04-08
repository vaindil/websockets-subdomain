using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebSockets.Data.Twitch;

namespace WebSockets.Data.Configs
{
    public class TwitchMarkerConfiguration : IEntityTypeConfiguration<TwitchMarker>
    {
        public void Configure(EntityTypeBuilder<TwitchMarker> builder)
        {
            builder.ToTable("twitch_stream");
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id).HasColumnName("id");
            builder.Property(m => m.StreamId).HasColumnName("twitch_stream_id").IsRequired();
            builder.Property(m => m.QueryTime).HasColumnName("query_time").IsRequired();
            builder.Property(m => m.MarkedBy).HasColumnName("marked_by").IsRequired();
            builder.Property(m => m.Reason).HasColumnName("reason");

            builder.HasOne(m => m.Stream)
                .WithMany(s => s.Markers)
                .HasForeignKey(m => m.StreamId);
        }
    }
}
