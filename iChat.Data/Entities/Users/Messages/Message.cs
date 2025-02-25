using iChat.ViewModels.Users.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Entities.Users.Messages
{
    public abstract class Message
    {
        public int MessageId { get; set; } 
        public AppUser? Sender { get; set; }
        public Guid SenderId { get; set; } 
        public MessageType MessageType { get; set; }
        public AppUser? Reciever { get; set; }
        public Guid ReceiverId { get; set; } 
        //public string Content { get; set; }
        public DateTime Timestamp { get; set; } 
    }
}
