using iChat.Data.Entities.Servers.ChatRoles;
using iChat.Data.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Configurations
{
    public class ChatRoleConfiguration : IEntityTypeConfiguration<ChatRole>
    {
        public void Configure(EntityTypeBuilder<ChatRole> b)
        {

                b.HasKey(r => r.Id);
                b.Property(r => r.Name).IsRequired();
                b.Property(r => r.Permissions)
                 .HasColumnType("bigint")
                 .IsRequired();
                b.HasIndex(r => r.Name).IsUnique();
                
                b.HasOne(cr=>cr.ChatServer)
                .WithMany(cs=>cs.ChatRoles)
                .HasForeignKey(cr => cr.ChatServerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
