using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Entities.Servers.ChatRoles
{
    public class ChatRole
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public Permission Permissions { get; set; }
        public long ChatServerId { get; set; }
        public ChatServer ChatServer { get; set; } = null!;
        public ICollection<UserChatRole> UserChatRoles { get; set; } = new HashSet<UserChatRole>();
        
        public ICollection<ChannelPermissionOverride> ChannelOverrides { get; set; } = new HashSet<ChannelPermissionOverride>();
    }
}
