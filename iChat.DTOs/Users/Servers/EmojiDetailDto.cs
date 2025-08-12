using iChat.DTOs.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Servers
{
    public class EmojiDetailDto :EmojiBaseDto
    {
        public EmojiDetailDto() { }
        public EmojiDetailDto(stringlong id, string name,ServerId serverId) : base(id, name)
        {
            this.ServerId = serverId;
        }

        public ServerId ServerId { get; set; }
        public EmojiBaseDto GetBase()
            => new (this.Id, this.Name);
    }
}
