using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Messages
{
   public class ChatMessageDto
    {
        public long Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string ContentMedia { get; set; } = string.Empty;
        public int MessageType { get; set; }
        public DateTime CreatedAt { get; set; }
        public long SenderId { get; set; }
   //     public string? SenderName { get; set; } = string.Empty;
    //   public string? SenderAvatarUrl { get; set; } = "https://cdn.discordapp.com/embed/avatars/0.png";
        
    }
}
