using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Messages
{
    public class ChatServerDto
    {
        string Id { get; set; } = string.Empty;
        string Name { get; set; } = string.Empty;
        string AvatarUrl { get; set; } = "https://cdn.discordapp.com/embed/avatars/0.png";
    }
}
