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
    public class TextMessageConfiguration : IEntityTypeConfiguration<TextMessage>
    {
        public void Configure(EntityTypeBuilder<TextMessage> builder)
        {
            builder.HasIndex(tm => tm.TextContent)
       .HasFilter("[TextContent] IS NOT NULL");
        }
    }
}
