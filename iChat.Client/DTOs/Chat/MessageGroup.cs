using iChat.Client.Services.UserServices.Chat;
using iChat.DTOs.Users;
using iChat.DTOs.Users.Messages;

namespace iChat.Client.DTOs.Chat
{
    public class MessageGroup
    {
        public string UserId { get; set; }
        public UserMetadataReact User { get; set; }
        public List<RenderedMessage> Messages { get; set; } = new();
        public DateTimeOffset Timestamp { get; set; }

        public bool CanAppend(ChatMessageDtoSafe nextMessage)
        {
            if (Messages.Count == 0) return false;
            if (nextMessage.SenderId != UserId)
                return false;
            var lastMessage = Messages[^1].Message;
            var timeGap = nextMessage.CreatedAt - lastMessage.CreatedAt;
            if (timeGap.TotalMinutes > 5)
                return false;

            return true;
        }
    }

}
