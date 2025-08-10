using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Entities.Users.Messages
{
    public class UserEmojiMessage
    {
        public long MessageId { get; set; }
        public Message Message { get; set; }
        public long UserId { get; set; }
        public AppUser User { get; set; }
        public long EmojiId {get;set;}
        public Emoji Emoji {get;set;}
    }
}
