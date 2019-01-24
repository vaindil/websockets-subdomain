using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebSockets.Data.ZubatRequests;

namespace WebSockets.Data.Configs.ZubatRequests
{
    public class SurvivorRequestConfiguration : IEntityTypeConfiguration<SurvivorRequest>
    {
        public void Configure(EntityTypeBuilder<SurvivorRequest> builder)
        {
            builder.ToTable("survivor_request");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.SurvivorName).HasColumnName("survivor_name").IsRequired();
            builder.Property(x => x.RequestText).HasColumnName("request_text").IsRequired();
            builder.Property(x => x.RequestedByTwitchId).HasColumnName("requested_by_twitch_id").IsRequired();
            builder.Property(x => x.RequestedAt).HasColumnName("requested_at");
            builder.Property(x => x.CompletedAt).HasColumnName("completed_at");
        }
    }
}
