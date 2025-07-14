﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Messages
{
    public class EditMessageRt
    {
      public  string ChannelId { get; set; } = string.Empty;
      public  string MessageId { get; set; } = string.Empty;
       public string NewContent { get; set; } = string.Empty;
       public string ServerId { get; set; } = string.Empty;
        public int BucketId { get; set; }
        public EditMessageRt()
        {
        }

        public EditMessageRt(long channelId, long messageId, string newContent, long serverId, int bucketId)
            {
                ChannelId = channelId.ToString();
                MessageId = messageId.ToString();
                NewContent = newContent;
                ServerId = serverId.ToString();
            BucketId = bucketId;
        }
    }
}
