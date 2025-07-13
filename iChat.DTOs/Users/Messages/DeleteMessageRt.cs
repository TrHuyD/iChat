using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Messages
{
    public class DeleteMessageRt
    {
     public   string ChannelId { get; set; } = string.Empty;
    public    string MessageId { get; set; } = string.Empty;
      public  string ServerId { get; set; } = string.Empty;
        public int BucketId { get; set; } 
        public DeleteMessageRt()
        {
        }
        public DeleteMessageRt(long channelId, long messageId,  long serverId, int bucketId)
        {
            ChannelId = channelId.ToString();
            MessageId = messageId.ToString();
            ServerId = serverId.ToString();
            BucketId = bucketId;
        }
    }
}
