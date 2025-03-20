using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Messages
{
    class ChatMessageRqDto
    {
        public long Id { get; set; }
        public string TextContent { get; set; } = string.Empty;
        public string[] MediaContent { get; set; } = Array.Empty<string>();
        public long SenderId { get; set; }
        public long ChannelId { get; set; }
    }
}
