using iChat.DTOs.Collections;
using iChat.DTOs.Users.Messages;
using iChat.DTOs.Users.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.ChatServerDatas
{
    public class ChatServerData
    {
        public ServerId Id { get; set; } = new();
        public string Name { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public List<ChatChannelDtoLite> Channels { get; set; } = new List<ChatChannelDtoLite>();
        public UserId AdminId { get; set; }
        public string Version { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
        public List<EmojiBaseDto> Emojis { get; set; } = new();
        
    }
}
