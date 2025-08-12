using iChat.Data.Entities.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Entities.Users.Messages
{
    public class Emoji
    {
        public long Id { get; set; }    
        public string Name { get; set; }
        public long ServerId { get; set; }
        public ChatServer ChatServer { get; set; }
    }
}
