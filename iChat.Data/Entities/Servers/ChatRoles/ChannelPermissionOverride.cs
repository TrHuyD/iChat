using iChat.Data.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Entities.Servers.ChatRoles
{
    public class ChannelPermissionOverride
    {
        public long Id { get; set; }
        public long ChannelId { get; set; }
        public ChatChannel ChatChannel { get; set; } = null!;   
        public long RoleId { get; set; }
        public ChatRole Role { get; set; } = null!;
       
        public Permission Allow { get; set; }
        public Permission Deny { get; set; }
    }
}
