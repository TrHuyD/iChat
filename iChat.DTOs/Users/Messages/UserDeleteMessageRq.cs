using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Messages
{
    public class UserDeleteMessageRq
    {
        public string ChannelId { set; get; }
        public string MessageId { set; get; }
        public string ServerId { set; get; }

    }
}
