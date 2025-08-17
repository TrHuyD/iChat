using iChat.Client.Services.UserServices.Chat.Util;
using iChat.DTOs.Collections;
using iChat.DTOs.Users.Servers;

namespace iChat.Client.DTOs.Chat
{
    public class EmojiFEDto
    {
        public string url { get; set; }

        public stringlong Id { get; set; }
        public string Name { get; set; }
        public EmojiFEDto(stringlong id,string name) { Id = id;Name = name; url = URLsanitizer.Apply($"/api/uploads/emojis/{id}.webp"); }
    }
}
