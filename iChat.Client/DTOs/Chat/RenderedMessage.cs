using iChat.DTOs.Users.Messages;

namespace iChat.Client.DTOs.Chat
{
    public class RenderedMessage
    {
        public ChatMessageDto Message { get; set; }
        public string CssClass { get; set; } = "";
        public string? Icon { get; set; }
        public string Content { get; set; } = "";
        public bool ShowTimestamp { get; set; }
        public bool isEdited { get; set; } = false;
        public static string DeleteMessage = "[This message has been deleted.]";
        public RenderedMessage WithDelete()
        {
            return new RenderedMessage
            {
                Message = new ChatMessageDto
                {
                    Id = Message.Id,
                    Content = DeleteMessage,
                    IsDeleted = true,
                    ChannelId=Message.ChannelId,
                    SenderId=Message.SenderId,
                    MessageType=Message.MessageType,
                    CreatedAt=Message.CreatedAt,

                },
                Content = DeleteMessage,
                CssClass = CssClass,
                Icon = Icon,
                ShowTimestamp = ShowTimestamp,
                isEdited = isEdited
            };
        }

        public RenderedMessage WithEdit(string newContent)
        {
            if (Message.IsDeleted) return this;

            return new RenderedMessage
            {
                Message = new ChatMessageDto
                {
                    Id = Message.Id,
                    Content = newContent,
                    IsDeleted = Message.IsDeleted,
                    ChannelId = Message.ChannelId,
                    SenderId = Message.SenderId,
                    MessageType = Message.MessageType,
                    CreatedAt = Message.CreatedAt,


                },
                Content = newContent,
                CssClass = CssClass,
                Icon = Icon,
                ShowTimestamp = ShowTimestamp,
                isEdited = true
            };
        }
    }

}
