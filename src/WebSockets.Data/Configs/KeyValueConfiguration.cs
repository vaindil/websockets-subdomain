using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebSockets.Data.Configs
{
    public class KeyValueConfiguration : IEntityTypeConfiguration<KeyValue>
    {
        public void Configure(EntityTypeBuilder<KeyValue> builder)
        {
            builder.ToTable("key_value");
            builder.HasKey(kv => kv.Key);

            builder.Property(kv => kv.Key).HasColumnName("key");
            builder.Property(kv => kv.Value).HasColumnName("value").IsRequired();
        }
    }
}
