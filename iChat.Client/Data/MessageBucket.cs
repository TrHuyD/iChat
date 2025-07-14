using iChat.DTOs.Users.Messages;
using System.Text.Json;

namespace iChat.Client.Data
{
    public class MessageBucket
    {

            public int BucketId { get; set; }
            public long ChannelId { get; set; }
            public int MessageCount { get; set; }
            public long FirstSequence { get; set; }
            public long LastSequence { get; set; }
            public List<ChatMessageDto> ChatMessageDtos { get; set; } = new List<ChatMessageDto>();
            public MessageBucket(BucketDto bucket)
            {
            BucketId = bucket.BucketId;
            ChannelId= long.Parse(bucket.ChannelId);
            MessageCount = bucket.MessageCount;
            if (bucket.ChatMessageDtos.Count == 0)
            {
                FirstSequence = 0;
                LastSequence = 0;
            }
            else
            {
                FirstSequence = long.Parse(bucket.FirstSequence);
                LastSequence = long.Parse(bucket.LastSequence);
            }
            ChatMessageDtos = bucket.ChatMessageDtos.Select(t => new ChatMessageDto(t)).ToList();
        }
        }
    }


