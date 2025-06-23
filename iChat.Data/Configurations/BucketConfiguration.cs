using iChat.Data.Entities.Users.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Configurations
{
    public class BucketConfiguration : IEntityTypeConfiguration<Bucket>
    {
        public void Configure(EntityTypeBuilder<Bucket> builder)
        {
            builder.HasKey(b => new { b.ChannelId, b.BucketId });

            builder.HasOne(b => b.Channel)
                .WithMany(c => c.Buckets)
                .HasForeignKey(b => b.ChannelId);

        }
    }
}
