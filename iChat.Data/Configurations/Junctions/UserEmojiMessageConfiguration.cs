using iChat.Data.Entities.Users.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Configurations.Junctions
{
    public class UserEmojiMessageConfiguration : IEntityTypeConfiguration<UserEmojiMessage>
    {
        public void Configure(EntityTypeBuilder<UserEmojiMessage> builder)
        {
            builder.HasKey(uem => new { uem.MessageId, uem.EmojiId, uem.UserId });
            builder.HasOne(uem => uem.Message)
                   .WithMany(m => m.EmojiReactions)
                   .HasForeignKey(uem => uem.MessageId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(uem => uem.User)
                   .WithMany()
                   .HasForeignKey(uem => uem.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(uem => uem.Emoji)
                   .WithMany() 
                   .HasForeignKey(uem => uem.EmojiId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(uem => uem.MessageId).IsDescending(true);
        }

            
    }
}
