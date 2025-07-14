using iChat.Data.Entities.Servers;
using iChat.ViewModels.Users.Messages;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace iChat.Data.Entities.Users.Messages
{
    public class Message
    {
        public long ChannelId { get; set; }
        public ChatChannel ChatChannel { get; set; }
        [Key]
        public long Id { get; set; }
        public long SenderId { get; set; }
        public AppUser User { get; set; }
        public short MessageType { get; set; }
        public string? TextContent { get; set; }
        public string? MediaContent { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public int BucketId { get; set; }
       public Bucket Bucket { get; set; } = null!;
        public NpgsqlTsVector? SearchVector { get; set; } 
        public DateTimeOffset? LastEditedAt { get; set; }
        public bool isDeleted { get; set; } 

    }
}
