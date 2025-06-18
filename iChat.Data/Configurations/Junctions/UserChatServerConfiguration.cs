using iChat.Data.Entities.Servers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Configurations.Junctions
{
    public class UserChatServerConfiguration : IEntityTypeConfiguration<UserChatServer>
    {
        public void Configure(EntityTypeBuilder<UserChatServer> builder)
        {
            builder.HasKey(ucs => new { ucs.UserId, ucs.ChatServerId });

            builder.HasOne(ucs => ucs.User)
                .WithMany(u => u.UserChatServers)
                .HasForeignKey(ucs => ucs.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ucs => ucs.ChatServer)
                .WithMany(cs => cs.UserChatServers)
                .HasForeignKey(ucs => ucs.ChatServerId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.HasIndex(ucs => ucs.ChatServerId);
            builder.HasIndex(ucs => ucs.UserId);
        }
    }
}
