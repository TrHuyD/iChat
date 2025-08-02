using iChat.DTOs.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Messages
{
    public class ChatChannelDto :ChatChannelDtoLite
    {
        public ServerId ServerId { get; set; }

        public ChatChannelDto(ChatChannelDtoLite dtoLite, ServerId serverId)
        {
            Id = dtoLite.Id;
            Name = dtoLite.Name;
            Order = dtoLite.Order;
            last_bucket_id = dtoLite.last_bucket_id;
            ServerId = serverId;
        }
        public ChatChannelDto()
        {
            Id = string.Empty;
            Name = string.Empty;
            ServerId = new();
            Order = 0;
            last_bucket_id = 0;
        }
    }
}
