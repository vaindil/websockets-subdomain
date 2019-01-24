using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebSockets.Data.ZubatRequests;

namespace WebSockets.Data.Configs.ZubatRequests
{
    public class RequestUserConfiguration : IEntityTypeConfiguration<RequestUser>
    {
        public void Configure(EntityTypeBuilder<RequestUser> builder)
        {
            builder.ToTable("request_user");
            builder.HasKey(x => x.TwitchId);

            builder.Property(x => x.TwitchId).HasColumnName("twitch_id");
            builder.Property(x => x.LastUsername).HasColumnName("last_username").IsRequired();
            builder.Property(x => x.RequestCount).HasColumnName("request_count");
        }
    }
}
