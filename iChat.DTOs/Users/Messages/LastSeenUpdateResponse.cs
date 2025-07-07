using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Messages
{
    public class LastSeenUpdateResponse
    {
        public string ServerId { get; set; }
        public string ChatChannelId { get; set; }
        public string MessageId { get; set; }
        public DateTimeOffset timestamp { get; set; } = DateTimeOffset.UtcNow;  
    }
}
