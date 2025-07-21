using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Entities.Servers
{
    public class UserChatChannel
    {
        public long UserId { get; set; }   
        public long ChannelId { get; set; }
        public long LastSeenMessage { get; set; }
        public int NotificationCount { get; set; }
    }
}
