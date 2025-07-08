using iChat.Client.Services.UserServices.Chat;

namespace iChat.Client.DTOs.Chat
{
    public class MessageGroup
    {
        public string UserId { get; set; }
        public UserMetadata User { get; set; }
        public List<RenderedMessage> Messages { get; set; } = new();
        public DateTimeOffset Timestamp { get; set; }
    }

}
