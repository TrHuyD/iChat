using iChat.Data.Entities.Servers;
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
    public class ChatServerConfiguration : IEntityTypeConfiguration<ChatServer>
    {
        public void Configure(EntityTypeBuilder<ChatServer> builder)
        {
            builder.HasKey(cs => cs.Id);

            builder.Property(cs => cs.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(cs => cs.Avatar)
                .HasMaxLength(300); 

            builder.Property(cs => cs.CreatedAt)
                .IsRequired();

            builder.Property(cs => cs.AdminId)
                .IsRequired();

            builder.HasOne(cs => cs.Admin)
                .WithMany() 
                .HasForeignKey(cs => cs.AdminId)
                .OnDelete(DeleteBehavior.Restrict); 

            builder.HasIndex(cs => cs.AdminId);
            builder.HasIndex(cs => cs.Name);
        }
    }
    
}
