using iChat.ViewModels.Users.Messages;

namespace iChat.BackEnd.Models.User.MessageRequests
{
    public class UserSendMessageRequest
    {

        public long ReceiveChannelId { get; set; }
        public IFormFile[]? files { get; set; }
        public string? Content { get; set; }
    }
}
