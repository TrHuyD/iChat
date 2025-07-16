using iChat.Client.DTOs.Chat;
using iChat.DTOs.Users.Messages;

namespace iChat.Client.Services.UserServices.Chat
{
    public class MessageRenderer
    {

        public static RenderedMessage RenderMessage(ChatMessageDto message)
        {
            var result = new RenderedMessage
            {
                Message = message,
                Icon = "",
                Content = message.Content,
                ShowTimestamp = true
            };
            if(message.IsDeleted)
            {
                return result.WithDelete();
            }
            return result;
        }
    }
}
