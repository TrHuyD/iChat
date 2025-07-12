using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Messages
{
    public class DeleteMessageRq
    {
     public   long UserId { get; set; }
      public  long ChannelId { get; set; }
       public long MessageId { get; set; }
        public long ServerId { get; set; }
        }
}
