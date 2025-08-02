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
        public ServerId serverId { get; set; }
        public ChannelId channelId { get; set; }
        public void Deconstruct(out ServerId serverId, out ChannelId channelId)
        {
            serverId = this.serverId;
            channelId = this.channelId;
        }
        public ChatServerConnectionState() 
        {
            serverId = new ServerId();
            channelId = new ChannelId();
        }
        public ChatServerConnectionState(ServerId longServerId, ChannelId longChannelId) { serverId = longServerId; channelId = longChannelId; }
        public ChatServerConnectionState(string serverId,string channelId) { this.channelId = new ChannelId(new stringlong(channelId));this.serverId = new ServerId(new stringlong(serverId)); }


    }


}