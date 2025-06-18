using iChat.Data.Entities.Servers.ChatRoles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Configurations
{
    public class ChannelPermissionOverrideConfiguration : IEntityTypeConfiguration<ChannelPermissionOverride>
    {
        public void Configure(EntityTypeBuilder<ChannelPermissionOverride> b)
        {
            b.HasKey(o => o.Id);

            b.HasOne(o => o.Role)
             .WithMany(r => r.ChannelOverrides)
             .HasForeignKey(o => o.RoleId)
             .OnDelete(DeleteBehavior.Cascade);

            b.Property(o => o.ChannelId).IsRequired();
            b.Property(o => o.Allow)
             .HasColumnType("bigint").IsRequired();
            b.Property(o => o.Deny)
             .HasColumnType("bigint").IsRequired();

            b.HasOne(cpo => cpo.ChatChannel)
             .WithMany(cc => cc.Overrides);
            b.HasIndex(o => new { o.ChannelId, o.RoleId }).IsUnique();
        }
    }
    
}
