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
    public class UserChatChannelConfiguration : IEntityTypeConfiguration<UserChatChannel>
    {
        public void Configure(EntityTypeBuilder<UserChatChannel> builder)
        {
            builder.HasKey(ucc => new { ucc.UserId, ucc.ChannelId });
            builder.Property(ucc => ucc.UserId)
                   .IsRequired();
            builder.Property(ucc => ucc.ChannelId)
                   .IsRequired();
            builder.Property(ucc => ucc.LastSeenMessage)
                   .IsRequired();
            builder.Property(ucc => ucc.NotificationCount)
                   .IsRequired();

        }
    }
}
