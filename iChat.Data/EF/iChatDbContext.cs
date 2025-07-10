using iChat.Data.Configurations;
using iChat.Data.Configurations.Junctions;
using iChat.Data.Entities.Servers;
using iChat.Data.Entities.Servers.ChatRoles;
using iChat.Data.Entities.Users;
using iChat.Data.Entities.Users.Auth;
using iChat.Data.Entities.Users.Messages;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.EF
{
    public partial class iChatDbContext(DbContextOptions options): IdentityDbContext<AppUser,Role,long>(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasPostgresExtension("pg_trgm");
            builder.ApplyConfiguration(new AppUserConfiguration());
            
            builder.ApplyConfiguration(new RefreshTokenConfiguration());
            builder.ApplyConfiguration(new MessageConfiguration());
            builder.ApplyConfiguration(new ChatServerConfiguration());
            builder.ApplyConfiguration(new ChatRoleConfiguration());
            builder.ApplyConfiguration(new UserChatRoleConfiguration());
            builder.ApplyConfiguration(new UserChatServerConfiguration());
            builder.ApplyConfiguration(new ChatChannelConfiguration());
            builder.ApplyConfiguration(new ChannelPermissionOverrideConfiguration());
            builder.ApplyConfiguration(new BucketConfiguration());
            builder.ApplyConfiguration(new ServerBanConfiguration());
            //builder.seed();
            base.OnModelCreating(builder);
        }
        
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ChatChannel> ChatChannels { get; set; }
        public DbSet<ChatServer> ChatServers { get; set; }
        public DbSet<ChatRole> ChatRoles { get; set; }
       public DbSet<Bucket> Buckets { get; set; }
        public DbSet<ChannelPermissionOverride> ChannelPermissionOverrides { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<UserChatRole> UserChatRoles { get; set; }
        public DbSet<UserChatServer> UserChatServers { get; set; }
        public DbSet<ServerBan> ServerBans { get; set; }
    }
}
