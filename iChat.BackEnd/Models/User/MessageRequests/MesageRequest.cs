using iChat.ViewModels.Users.Messages;

namespace iChat.BackEnd.Models.User.MessageRequests
{
    public class MesageRequest
    {
        public long? SenderId { get; set; }
        
        public MessageType? messageType { get; set; }
        public long ReceiveChannelId { get; set; }
        public string? MediaContent { get; set; }
        public string? TextContent { get; set; }
    }
}
