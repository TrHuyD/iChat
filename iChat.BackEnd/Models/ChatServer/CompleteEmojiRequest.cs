using iChat.DTOs.Collections;

namespace iChat.BackEnd.Models.ChatServer
{
    public class CompleteEmojiRequest
    {
       public IFormFile File { get; set; }
       public ServerId ServerId { get; set; }
       public UserId UserId { get; set; }
    
    }
}
