using iChat.Client.DTOs.Chat;
using iChat.Client.Services.UserServices.Chat.Util;
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
                ShowTimestamp = true
            };

            if (message.IsDeleted)
                return result;

            if (message.ContentMedia != null)
            {
                var url = URLsanitizer.Apply(message.ContentMedia.Url);
                result.Content = @$"<img src=""{url}"" style=""max-height:400px; height:auto; width:auto;"" />";
            }
            else
            {
                result.Content = message.Content; 
            }

            return result;
        }

    }
}
