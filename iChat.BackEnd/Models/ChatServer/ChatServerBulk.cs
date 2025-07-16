using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Models.ChatServer
{
    public class ChatServerbulk
    {
        public List<long>  memberList;
        public long Id { get; set; } 
        public string Name { get; set; } 
        public string AvatarUrl { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public List<ChatChannelDtoLite> Channels { get; set; } = new List<ChatChannelDtoLite>();
        public long AdminId { get; set; } 
    }
}
