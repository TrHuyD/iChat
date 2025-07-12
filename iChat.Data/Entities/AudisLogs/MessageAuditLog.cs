using iChat.Data.Entities.Servers;
using iChat.Data.Entities.Users;
using iChat.Data.Entities.Users.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Entities.Logs
{
    public class MessageAuditLog
    {
        [Key]
        public long Id { get; set; }
        public long ChannelId { get; set; }
        public ChatChannel ChatChannel { get; set; } = null!;
        public long MessageId { get; set; }
        public Message Message { get; set; } = null!;
        public AuditActionType ActionType { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string PreviousContent { get; set; } 
        public long ActorUserId { get; set; }
        public AppUser ActorUser { get; set; } = null!;
    }

}
