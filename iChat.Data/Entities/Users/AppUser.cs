using iChat.Data.Entities.Servers;
using iChat.Data.Entities.Servers.ChatRoles;
using iChat.Data.Entities.Users.Auth;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Entities.Users
{
    public class AppUser : IdentityUser<long>
    {
        public string Name { get; set; }
        public DateTime Dob { get; set; }
        
        public ICollection<RefreshToken> RefreshTokens { get; set; }
        public DateTimeOffset LastSeen { get; set; }
        public ICollection<UserChatServer> UserChatServers { get; set; } = new List<UserChatServer>();

        public ICollection<UserChatRole> UserRoles { get; set; } = new HashSet<UserChatRole>();

    }
}
