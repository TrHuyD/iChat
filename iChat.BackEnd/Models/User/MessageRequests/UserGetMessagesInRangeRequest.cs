namespace iChat.BackEnd.Models.User.MessageRequests
{
    public class UserGetMessagesInRangeRequest
    {
        public long StartId { get; set; }
    public     long EndId { get; set; }
     public   long ChannelId { get; set; }
    }
}
