﻿using iChat.Data.Entities.Users;
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

            builder.HasOne(m => m.User)
                   .WithMany()
                   .HasForeignKey(m => m.SenderId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(m => m.MediaFile)
                   .WithMany() 
                   .HasForeignKey(m => m.MediaId)
                   .OnDelete(DeleteBehavior.SetNull);
          //  builder.HasIndex(m => m.ChannelId);
            builder.HasIndex(m => new { m.ChannelId, m.Timestamp }).IsDescending(false, true);
            builder.HasIndex(m => new { m.ChannelId, m.SenderId, m.Timestamp }).IsDescending(false, false, true);
            builder.HasIndex(m => new { m.ChannelId, m.BucketId,m.Id }).IsDescending(false, true, true);

            builder.Property(m => m.MessageType)
                   .HasColumnType("smallint");
            builder.HasOne(m => m.ChatChannel)
                .WithMany()
                .HasForeignKey(m => m.ChannelId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Property(m => m.BucketId)
                .HasDefaultValue(int.MaxValue);
            builder.HasOne(m => m.Bucket)
                .WithMany(b => b.Messages)
                .HasForeignKey(m => new { m.ChannelId, m.BucketId });
            builder.Property(m => m.SearchVector)
                .HasComputedColumnSql(
                    "CASE WHEN NOT \"isDeleted\" THEN to_tsvector('english', coalesce(\"TextContent\", '')) ELSE NULL END",
                    stored: true);
            builder.HasIndex(m => m.SearchVector)
                .HasMethod("GIN");

        }
    }
}
