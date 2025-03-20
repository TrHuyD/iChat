using iChat.ViewModels.Users.Messages;

namespace iChat.BackEnd.Models.User.MessageRequests
{
    public class MesageRequest :BaseMessageRequest
    {
        public long? MessageId { get; set; }
        public long? SenderId { get; set; }
        
        public MessageType? messageType { get; set; }
    }
}
