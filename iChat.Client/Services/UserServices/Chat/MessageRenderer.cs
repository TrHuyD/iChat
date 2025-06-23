using iChat.DTOs.Users.Messages;

namespace iChat.Client.Services.UserServices.Chat
{
    public class MessageRenderer
    {
        public class RenderedMessage
        {
            public string Content { get; set; } = string.Empty;
            public string CssClass { get; set; } = string.Empty;
            public string Icon { get; set; } = string.Empty;
            public bool ShowTimestamp { get; set; } = true;
            public bool IsSystemMessage { get; set; } = false;
        }

        public static RenderedMessage RenderMessage(ChatMessageDtoSafe message, string currentUserId)
        {
            var rendered = new RenderedMessage();
            Console.Write("Rendering message: " + message.Id+message.CreatedAt + "\n");
            switch (message.MessageType)
            {
                case 1: // Regular text message
                    rendered.Content = message.Content;
                    rendered.CssClass = message.SenderId == currentUserId ? "message sent" : "message received";
                    break;

                case 2: // System message
                    rendered.Content = message.Content;
                    rendered.CssClass = "message system";
                    rendered.IsSystemMessage = true;
                    rendered.Icon = "🔔";
                    break;

                case 3: // User joined
                    rendered.Content = $"{GetUserDisplayName(message)} joined the chat";
                    rendered.CssClass = "message system join";
                    rendered.IsSystemMessage = true;
                    rendered.Icon = "👋";
                    break;

                case 4: // User left
                    rendered.Content = $"{GetUserDisplayName(message)} left the chat";
                    rendered.CssClass = "message system leave";
                    rendered.IsSystemMessage = true;
                    rendered.Icon = "👋";
                    break;

                case 5: // File/Image message
                    rendered.Content = ProcessFileMessage(message);
                    rendered.CssClass = message.SenderId == currentUserId ? "message sent file" : "message received file";
                    rendered.Icon = "📎";
                    break;

                case 6: // Emoji reaction
                    rendered.Content = message.Content;
                    rendered.CssClass = "message reaction";
                    rendered.ShowTimestamp = false;
                    break;

                case 7: // Typing indicator
                    rendered.Content = $"{GetUserDisplayName(message)} is typing...";
                    rendered.CssClass = "message typing";
                    rendered.IsSystemMessage = true;
                    rendered.ShowTimestamp = false;
                    rendered.Icon = "⌨️";
                    break;

                default:
                    rendered.Content = message.Content;
                    rendered.CssClass = message.SenderId == currentUserId ? "message sent" : "message received";
                    break;
            }

            return rendered;
        }

        private static string GetUserDisplayName(ChatMessageDtoSafe message)
        {
            // You can enhance this to fetch actual user names from a service
            return $"User {message.SenderId}";
        }

        private static string ProcessFileMessage(ChatMessageDtoSafe message)
        {
            // Process file messages - could include file preview, download links, etc.
            // For now, just return the content as-is
            return message.Content;
        }

        public static bool ShouldShowMessage(ChatMessageDtoSafe message)
        {
            // Filter out messages that shouldn't be displayed
            // For example, temporary typing indicators, etc.
            return message.MessageType != 7; // Don't show typing indicators in chat history
        }
    }
}
