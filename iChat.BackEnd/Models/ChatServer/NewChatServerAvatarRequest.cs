using Microsoft.AspNetCore.Mvc;

namespace iChat.BackEnd.Models.ChatServer
{
    public class NewChatServerAvatarRequest
    {
        [FromForm(Name = "file")]
        public IFormFile File { get; set; }
        [FromForm(Name = "id")]
        public string ServerId { get; set; }
    }
}
