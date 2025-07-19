using iChat.DTOs.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Messages
{
    public class ChatServerChangeUpdate
    {
        public stringlong Id { get; set; }
       public string AvatarUrl { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
    }
}
