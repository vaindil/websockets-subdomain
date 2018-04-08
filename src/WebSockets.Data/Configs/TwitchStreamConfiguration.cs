using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebSockets.Data.Twitch;

namespace WebSockets.Data.Configs
{
    public class TwitchStreamConfiguration : IEntityTypeConfiguration<TwitchStream>
    {
        public void Configure(EntityTypeBuilder<TwitchStream> builder)
        {
            builder.ToTable("twitch_stream");
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id).HasColumnName("id");
            builder.Property(s => s.ChannelId).HasColumnName("channel_id").IsRequired();
            builder.Property(s => s.ChannelName).HasColumnName("channel_name").IsRequired();
            builder.Property(s => s.VodId).HasColumnName("vod_id");

            builder.HasIndex(s => s.VodId).IsUnique();
        }
    }
}
