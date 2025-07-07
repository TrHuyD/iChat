using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Messages
{
    public class ChatServerDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = "https://cdn.discordapp.com/embed/avatars/0.png";
        public int Position { get; set; } = 0;
        public bool isadmin { get; set; } = false;
        public List<ChatChannelDtoLite> Channels { get; set; } = new List<ChatChannelDtoLite>();
    }
}
