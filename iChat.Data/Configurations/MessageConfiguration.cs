using iChat.Data.Entities.Users;
using iChat.Data.Entities.Users.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Configurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.ToTable("Messages");

            // Primary Key
            builder.HasKey(m => m.Id);

            // Required fields
            builder.Property(m => m.Id).ValueGeneratedNever(); 
            builder.Property(m => m.ChannelId).IsRequired();
            builder.Property(m => m.SenderId).IsRequired();
            builder.Property(m => m.Timestamp).IsRequired();

            // Optional fields
            builder.Property(m => m.TextContent).HasMaxLength(40000);
            builder.Property(m => m.MediaContent).HasMaxLength(2048);

            builder.HasOne(m => m.User)
                   .WithMany()
                   .HasForeignKey(m => m.SenderId)
                   .OnDelete(DeleteBehavior.Restrict); 

            builder.HasIndex(m => m.ChannelId);
            builder.HasIndex(m => new { m.ChannelId, m.Timestamp });
            builder.HasIndex(m => new { m.ChannelId, m.Id });
            builder.HasIndex(m => new { m.ChannelId, m.SenderId, m.Timestamp });
            builder.Property(m => m.MessageType)
                   .HasColumnType("smallint");
        }
    }
}
