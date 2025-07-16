using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Messages
{
    public class MessageProcessingContext
    {
        public bool IsInServer { get; set; }
        public UserMetadata? Metadata { get; set; }
        public List<long>? UserServerList { get; set; }
    }
}
