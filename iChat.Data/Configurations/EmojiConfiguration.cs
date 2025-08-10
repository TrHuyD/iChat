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
    public class EmojiConfiguration : IEntityTypeConfiguration<Emoji>
    {
        public void Configure(EntityTypeBuilder<Emoji> builder)
        {
            builder.HasOne(e => e.ChatServer)
                    .WithMany(cs => cs.Emojis)
                    .HasForeignKey(e => e.ServerId);

        }
    }
}
