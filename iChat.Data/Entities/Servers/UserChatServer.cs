using iChat.Data.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Entities.Servers
{
    public class UserChatServer
    {
       public ChatServer ChatServer { get; set; }
        public long ChatServerId { get; set; }
        public AppUser User { get; set; }
        public long UserId { get; set; }
        public ICollection<UserChatServer> UserChatServers { get; set; } = new List<UserChatServer>();
    }
}
