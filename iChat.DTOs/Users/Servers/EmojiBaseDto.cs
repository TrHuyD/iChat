using iChat.DTOs.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Servers
{
    public class EmojiBaseDto
    {
        public EmojiBaseDto() { }
        public EmojiBaseDto(stringlong id, string name)
        {
            Id = id;
            Name = name;
        }

        public stringlong Id { get; set; }
        public string Name { get; set; }
    }
}
