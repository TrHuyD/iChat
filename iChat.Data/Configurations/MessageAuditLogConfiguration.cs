using iChat.Data.Entities.Logs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Configurations
{
    public class MessageAuditLogConfiguration : IEntityTypeConfiguration<MessageAuditLog>
    {
        public void Configure(EntityTypeBuilder<MessageAuditLog> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.ChannelId)
                   .IsRequired();
            builder.Property(e => e.MessageId)
                   .IsRequired();
            builder.Property(e => e.PreviousContent)
                     .IsRequired().HasMaxLength(40000);
            builder.Property(e => e.ActionType)
                   .HasConversion<short>()
                   .IsRequired();
            builder.Property(e => e.Timestamp)
                   .IsRequired();
            builder.Property(e => e.ActorUserId)
                   .IsRequired();
            builder.HasOne(e => e.Message)
                   .WithMany()
                   .HasForeignKey(e => e.MessageId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.ActorUser)
                   .WithMany()
                   .HasForeignKey(e => e.ActorUserId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.ChatChannel)
                   .WithMany()
                   .HasForeignKey(e => e.ChannelId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(e => e.ChannelId);
            builder.HasIndex(e => e.MessageId);
            builder.HasIndex(e => new { e.ChannelId, e.ActionType });
            builder.HasIndex(e => new { e.ActorUserId, e.Timestamp });
        }
    }
}