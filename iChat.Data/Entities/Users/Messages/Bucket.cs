using iChat.Data.Entities.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Entities.Users.Messages
{
    public class Bucket
    {
        public long ChannelId { get; set; }
        public int BucketId { get; set; }
        public ChatChannel Channel { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }

        public ICollection<Message> Messages { get; set; } = new List<Message>();

    }
}
