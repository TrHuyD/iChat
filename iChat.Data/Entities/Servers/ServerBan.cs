using iChat.Data.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Entities.Servers
{
    public class ServerBan 
    {
        public long UserId { get; set; }
        public AppUser User { get; set; }
        public long ChatServerId { get; set; }
        public ChatServer ChatServer { get; set; }
        public string? Reason { get; set; }
        public DateTimeOffset BannedAt { get; set; }
    }
}
