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
        public long last_seen { get; set; } // Last seen message ID or timestamp
        public long ChatServerId { get; set; }
        public AppUser User { get; set; }
        public long UserId { get; set; }
        public short Order { get; set; } // Order of the user in the server, for sorting purposes
        public DateTimeOffset JoinedAt { get; set; } // When the user joined the server
    }
}
