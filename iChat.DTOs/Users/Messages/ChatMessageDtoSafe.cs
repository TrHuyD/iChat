using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Messages
{
    public class ChatMessageDtoSafe
    {
        public string Id { get; set; }
        [JsonIgnore]
        private long? _idLong;  
        [JsonIgnore]
        public long IdLong =>_idLong??= long.TryParse(Id, out var result) ? result : throw new Exception("unexpected value");
        public string Content { get; set; } = string.Empty;
        public string ContentMedia { get; set; } = string.Empty;
        public int MessageType { get; set; }
        //   [JsonConverter(typeof(DateTimeOffsetJsonConverter))]
        public DateTimeOffset CreatedAt { get; set; }
        public string SenderId { get; set; }
        [JsonIgnore]
        private long? _senderIdLong;
        [JsonIgnore]
        public long SenderIdLong =>_senderIdLong??= long.TryParse(SenderId, out var result) ? result : throw new Exception("unexpected value");
        public string ChannelId { get; set; }
        [JsonIgnore]
        private long? _channelIdLong;
        [JsonIgnore]
        public long ChannelIdLong =>_channelIdLong??= long.TryParse(ChannelId, out var result) ? result : throw new Exception("unexpected value");
        //     public string? SenderName { get; set; } = string.Empty;
        //   public string? SenderAvatarUrl { get; set; } = "https://cdn.discordapp.com/embed/avatars/0.png";
        public  bool isEdited { get; set; }
        public  bool isDeleted { get; set; }
        public ChatMessageDtoSafe(ChatMessageDto chatMessageDto)
        {
            Id = chatMessageDto.Id.ToString();
            Content = chatMessageDto.Content;
            ContentMedia = chatMessageDto.ContentMedia;
            MessageType = chatMessageDto.MessageType;
            CreatedAt = chatMessageDto.CreatedAt;
            SenderId = chatMessageDto.SenderId.ToString();
            ChannelId = chatMessageDto.RoomId.ToString();
            isDeleted= chatMessageDto.IsDeleted; 
            isEdited= chatMessageDto.IsEdited; 

        }
        public ChatMessageDtoSafe()
        {

        }
        public ChatMessageDto ToChatMessageDto()
        {
            return new ChatMessageDto
            {
                Id = long.Parse(Id),
                Content = Content,
                ContentMedia = ContentMedia,
                MessageType = MessageType,
                CreatedAt = CreatedAt,
                SenderId = long.Parse(SenderId),
                RoomId = long.Parse(ChannelId),
                IsEdited = isEdited,
                IsDeleted = isDeleted
            };
        }
    }
    public class ChatMessageDtoSafeComparer : IComparer<ChatMessageDtoSafe>
    {
        public int Compare(ChatMessageDtoSafe? x, ChatMessageDtoSafe? y)
        {
            if (x == null || y == null) return 0;
            return x.CreatedAt.CompareTo(y.CreatedAt);
        }
    }
    public class ChatMessageDtoSafeSearchExtended : ChatMessageDtoSafe
    {
        public int BucketId { get; set; } = int.MaxValue; // Default to MaxValue for search purposes

        public ChatMessageDtoSafeSearchExtended()
        {
        }
    }
}
