using iChat.Data.Entities.Servers.ChatRoles;
using iChat.Data.Entities.Users.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Entities.Servers
{
    public class ChatChannel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public long ServerId { get; set; }
        public short Order { get; set; } = 0;
        //public long ChatServerId {get;set;}
        public ChatServer Server { get; set; }
        public ICollection<ChannelPermissionOverride> Overrides { get; set; } = new List<ChannelPermissionOverride>();
        public ICollection<Bucket> Buckets { get; set; } = new List<Bucket>();
        public int LastAssignedBucketId { get; set; } = 0;
    }
}
