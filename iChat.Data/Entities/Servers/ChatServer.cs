using iChat.Data.Entities.Servers.ChatRoles;
using iChat.Data.Entities.Users;
using iChat.Data.Entities.Users.Messages;

namespace iChat.Data.Entities.Servers
{
    public class ChatServer
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Avatar { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public long AdminId {get;set;}
        public AppUser Admin { get; set; }
        public ICollection<ChatRole> ChatRoles { get; set; } = new List<ChatRole>();
        public ICollection<ChatChannel> ChatChannels { get; set; } = new List<ChatChannel>();
        public ICollection<UserChatServer> UserChatServers { get; set; } = new List<UserChatServer>();
        public ICollection<ServerBan> Bans { get; set; } = new List<ServerBan>();
        public ICollection<Emoji> Emojis { get; set; } = new List<Emoji>();

    }
}
