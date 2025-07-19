using iChat.DTOs.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users
{
    public class ChatServerConnectionState
    {
        public stringlong ServerId { get; set; }
        public stringlong ChannelId { get; set; }
        public void Deconstruct(out stringlong serverId, out stringlong channelId)
        {
            serverId = ServerId;
            channelId = ChannelId;
        }
        public ChatServerConnectionState() { }
        public ChatServerConnectionState(stringlong longServerId, stringlong longChannelId) { ServerId = longServerId;ChannelId = longChannelId; }


    }


}