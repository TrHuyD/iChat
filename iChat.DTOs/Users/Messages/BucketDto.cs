using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Messages
{
    public class BucketDto
    {
        public int BucketId { get; set; }
        public long ChannelId { get; set; }
        public long MessageCount { get; set; }
        public long FirstSequence { get; set; }
        public long LastSequence { get; set; }
        public List<ChatMessageDto> ChatMessageDtos { get; set; } = new List<ChatMessageDto>();
        public BucketDto(RawBucketResult r)
        {
            BucketId = r.bucket_id;
            ChannelId = r.channel_id;
            MessageCount = r.message_count;

            ChatMessageDtos = JsonSerializer.Deserialize<List<ChatMessageDto>>(r.messages);
            ChatMessageDtos.Sort((a, b) => a.Id.CompareTo(b.Id));
            if(ChatMessageDtos.Count == 0)
            {
                FirstSequence = 0;
                LastSequence = 0;
                return;
            }
            FirstSequence = ChatMessageDtos[0].Id ;
            LastSequence = ChatMessageDtos[^1].Id;

        }
    }
}
