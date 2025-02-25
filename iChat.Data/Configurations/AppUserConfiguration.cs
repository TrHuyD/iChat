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
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.Property(x => x.FirstName).IsRequired().HasMaxLength(15);
            builder.Property(x => x.LastName).IsRequired().HasMaxLength(15);
            builder.Property(x => x.Dob).IsRequired();
        }
    }
}
