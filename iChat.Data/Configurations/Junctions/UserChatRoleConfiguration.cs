using iChat.Data.Entities.Servers.ChatRoles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Configurations.Junctions
{
    public class UserChatRoleConfiguration : IEntityTypeConfiguration<UserChatRole>
    {
        public void Configure(EntityTypeBuilder<UserChatRole> b)
        {
            b.HasKey(ur => new { ur.UserId, ur.RoleId });

            b.HasOne(ur => ur.User)
             .WithMany(u => u.UserRoles)
             .HasForeignKey(ur => ur.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(ur => ur.Role)
             .WithMany(r => r.UserChatRoles)
             .HasForeignKey(ur => ur.RoleId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
