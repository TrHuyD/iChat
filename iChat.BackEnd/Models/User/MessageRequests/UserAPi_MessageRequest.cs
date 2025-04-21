using iChat.ViewModels.Users.Messages;

namespace iChat.BackEnd.Models.User.MessageRequests
{
    public class UserAPi_MessageRequest
    {
        public MessageType messageType { get; set; }
        public string? MediaContent { get; set; }
        public string? TextContent { get; set; }
        public long ReceiveChannelId { get; set; }
    }
}
