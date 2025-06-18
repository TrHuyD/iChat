using iChat.Data.Entities.Servers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Configurations
{
    public class ChatChannelConfiguration : IEntityTypeConfiguration<ChatChannel>
    {
        public void Configure(EntityTypeBuilder<ChatChannel> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.CreatedAt)
                .IsRequired();

            builder.Property(c => c.ServerId)
                .IsRequired();

            builder.HasOne(c => c.Server)
                .WithMany(s => s.ChatChannels)
                .HasForeignKey(c => c.ServerId)
                .OnDelete(DeleteBehavior.Cascade);

            
            builder.HasIndex(c => c.ServerId);
        }
    }

}
