using iChat.ViewModels.Users.Messages;

namespace iChat.BackEnd.Models.User.MessageRequests
{
    public class MessageRequest
    {
        public long? MessageId { get; set; }
        public long? SenderId { get; set; }
        //public AppUser? Reciever { get; set; }
        public long ReceiveChannelId { get; set; }

        public IFormFile[]? files { get; set; }
        public string? Content { get; set; }
    }
}
