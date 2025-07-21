using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Messages
{
    public class UserEditMessageRq
    {
        public string ServerId { get; set; }
      public  string ChannelId { get; set; }
      public  string MessageId { get; set; }
        public string NewContent { get; set; }

    }
}
