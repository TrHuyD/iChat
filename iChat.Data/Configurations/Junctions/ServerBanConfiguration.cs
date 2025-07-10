using iChat.Data.Entities.Servers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Configurations.Junctions
{
    public class ServerBanConfiguration : IEntityTypeConfiguration<ServerBan>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<ServerBan> builder)
        {
            builder.HasKey(x=> new { x.UserId, x.ChatServerId });
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.BannedBy)
                .WithMany()
                .HasForeignKey(x => x.BannedById)
                .OnDelete(DeleteBehavior.Restrict); // Restrict to prevent deletion of the user who banned
            builder.HasOne(x => x.ChatServer)
                .WithMany(s=>s.Bans)
                .HasForeignKey(x => x.ChatServerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
