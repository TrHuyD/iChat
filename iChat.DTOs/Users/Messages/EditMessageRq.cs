using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Messages
{
    public class EditMessageRq
    {
        public long UserId { get; set; }
        public long ChannelId { get; set; }
        public long MessageId { get; set; }
        public string NewContent { get; set; } = string.Empty;
        public EditMessageRq()
        {

        }
    }
}
