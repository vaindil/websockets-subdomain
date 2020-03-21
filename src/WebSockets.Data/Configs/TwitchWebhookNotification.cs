using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebSockets.Data.Configs
{
    public class TwitchWebhookNotificationConfiguration : IEntityTypeConfiguration<TwitchWebhookNotification>
    {
        public void Configure(EntityTypeBuilder<TwitchWebhookNotification> builder)
        {
            builder.ToTable("twitch_webhook_notification");
            builder.HasKey(n => n.Id);

            builder.Property(n => n.Id).HasColumnName("id");
            builder.Property(n => n.ReceivedAt).HasColumnName("received_at").IsRequired();
            builder.Property(n => n.UserId).HasColumnName("user_id").IsRequired();
            builder.Property(n => n.Username).HasColumnName("username").IsRequired();
            builder.Property(n => n.GameId).HasColumnName("game_id");
            builder.Property(n => n.Title).HasColumnName("title");
            builder.Property(n => n.StartedAt).HasColumnName("started_at");
        }
    }
}
