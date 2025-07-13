using iChat.Client.DTOs.Chat;
using iChat.DTOs.Users.Messages;

namespace iChat.Client.Services.UserServices.Chat
{
    public class MessageRenderer
    {

        public static RenderedMessage RenderMessage(ChatMessageDtoSafe message, string currentUserId)
        {
            var result = new RenderedMessage
            {
                Message = message,
                CssClass = message.SenderId == currentUserId ? "message-self" : "message-other",
                Icon = "",
                Content = message.Content,
                ShowTimestamp = true
            };
            if(message.isDeleted)
            {
                result.ToggleDelete();
            }
            return result;
        }
    }
}
