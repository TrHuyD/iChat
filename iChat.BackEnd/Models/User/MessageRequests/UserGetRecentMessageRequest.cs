namespace iChat.BackEnd.Models.User.MessageRequests
{
    public class UserGetRecentMessageRequest
    {
        public long UserId { get; set; }
        public long ChannelId { get; set; }
        public long? LastMessageId { get; set; }
    }
}
