using iChat.DTOs.Users.Messages;

namespace iChat.Client.DTOs.Chat
{
    public class RenderedMessage
    {
        public ChatMessageDtoSafe Message { get; set; }
        public string CssClass { get; set; } = "";
        public string? Icon { get; set; }
        public string Content { get; set; } = "";
        public bool ShowTimestamp { get; set; }
    
    }

}
