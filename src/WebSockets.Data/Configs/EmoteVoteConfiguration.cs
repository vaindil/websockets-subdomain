using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebSockets.Data.Configs
{
    public class EmoteVoteConfiguration : IEntityTypeConfiguration<EmoteVote>
    {
        public void Configure(EntityTypeBuilder<EmoteVote> builder)
        {
            builder.ToTable("emote_vote");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.IpAddress).HasColumnName("ip_address").IsRequired();
            builder.Property(x => x.LoggedAt).HasColumnName("logged_at").IsRequired();
            builder.Property(x => x.EmoteName).HasColumnName("emote_name").IsRequired();
            builder.Property(x => x.IsVoteForNew).HasColumnName("is_vote_for_new").IsRequired();
        }
    }
}
