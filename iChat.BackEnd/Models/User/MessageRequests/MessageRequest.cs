using iChat.DTOs.Users;
using iChat.ViewModels.Users.Messages;

namespace iChat.BackEnd.Models.User.MessageRequests
{
    public class MessageRequest
    {
        public string SenderId { get; set; }
        public string ReceiveChannelId { get; set; }
        public string? TextContent { get; set; }
        public MediaFileDto? MediaFileMetaData { get; set; }
    }
}
