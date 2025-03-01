using iChat.Data.Configurations;
using iChat.Data.Entities.Users;
using iChat.Data.Entities.Users.Auth;
using iChat.Data.Entities.Users.Messages;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.EF
{
    public class iChatDbContext(DbContextOptions options): IdentityDbContext<AppUser,Role,Guid>(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new AppUserConfiguration());
            
            builder.ApplyConfiguration(new MessageConfiguration());
            builder.ApplyConfiguration(new TextMessageConfiguration());
            builder.ApplyConfiguration(new RefreshTokenConfiguration());
            //builder.seed();
            base.OnModelCreating(builder);
        }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Message> Messages { get; set; }
    }
}
