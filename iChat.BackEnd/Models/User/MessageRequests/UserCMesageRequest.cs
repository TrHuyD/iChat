namespace iChat.BackEnd.Models.User.MessageRequests
{
    public class UserCSendMessage : UserSendMessageRequest
    {
        public long SenderId { get; set; }
        public long ReceiveChannelId { get; set; }
        public int MessageType { get; set; }
    }
}
