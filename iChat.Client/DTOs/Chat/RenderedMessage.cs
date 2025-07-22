using iChat.DTOs.Users.Messages;

namespace iChat.Client.DTOs.Chat
{
    public class RenderedMessage
    {
        public int GroupCount { get; set; }
        public ChatMessageDto Message { get; set; }
        public string CssClass { get; set; } = "";
        public string? Icon { get; set; }
        public string Content { get; set; } = "";
        public bool ShowTimestamp { get; set; }
        public static string DeleteMessage = "[This message has been deleted.]";
        public UserMetadataReact User { get; set; }
        //public RenderedMessage WithDelete()
        //{
        //    return new RenderedMessage
        //    {
        //        Message = new ChatMessageDto
        //        {
        //            Id = Message.Id,
        //            Content = DeleteMessage,
        //            IsDeleted = true,
        //            ChannelId=Message.ChannelId,
        //            SenderId=Message.SenderId,
        //            MessageType=Message.MessageType,
        //            CreatedAt=Message.CreatedAt,

        //        },
        //        Content = DeleteMessage,
        //        CssClass = CssClass,
        //        Icon = Icon,
        //        ShowTimestamp = ShowTimestamp,
        //        isEdited = isEdited
        //        ,GroupCount=GroupCount,
        //        User=User
        //    };
        //}

        //public RenderedMessage WithEdit(string newContent)
        //{
        //    if (Message.IsDeleted) return this;
        //    isEdited = true;
        //    Content = newContent;
        //    return new RenderedMessage
        //    {
        //        Message = new ChatMessageDto
        //        {
        //            Id = Message.Id,
        //            Content = newContent,
        //            IsDeleted = Message.IsDeleted,
        //            ChannelId = Message.ChannelId,
        //            SenderId = Message.SenderId,
        //            MessageType = Message.MessageType,
        //            CreatedAt = Message.CreatedAt,


        //        },
        //        Content = newContent,
        //        CssClass = CssClass,
        //        Icon = Icon,
        //        ShowTimestamp = ShowTimestamp,
        //        isEdited = true,
        //        GroupCount = GroupCount,
        //        User = User
        //    };
        //}
    }

}
