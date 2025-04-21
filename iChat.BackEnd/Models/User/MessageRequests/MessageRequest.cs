using iChat.ViewModels.Users.Messages;

namespace iChat.BackEnd.Models.User.MessageRequests
{
    public class MessageRequest
    {
        public string SenderId { get; set; }
        
        public MessageType messageType { get; set; }
        public string ReceiveChannelId { get; set; }
        public string? MediaContent { get; set; }
        public string? TextContent { get; set; }
    }
}
