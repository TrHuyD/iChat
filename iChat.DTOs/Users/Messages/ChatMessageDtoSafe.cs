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
     
        public string Content { get; set; } = string.Empty;
        public string ContentMedia { get; set; } = string.Empty;
        public int MessageType { get; set; }
        //   [JsonConverter(typeof(DateTimeOffsetJsonConverter))]
        public DateTimeOffset CreatedAt { get; set; }
        public string SenderId { get; set; }
        
        public string ChannelId { get; set; }
       
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
            ChannelId = chatMessageDto.ChannelId.ToString();
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
                ChannelId = long.Parse(ChannelId),
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
    public class NewMessage : ChatMessageDtoSafe
    {
        public required string UserMetadataVersion { get; init; }
        public NewMessage(ChatMessageDto chatMessageDto, long userMetadataVersion) : base(chatMessageDto)
        {
            UserMetadataVersion = userMetadataVersion.ToString();
        }
        public NewMessage()
        {
        }
    }
    public class ChatMessageDtoSafeSearchExtended : ChatMessageDtoSafe
    {
        public int BucketId { get; set; } = int.MaxValue;

        public ChatMessageDtoSafeSearchExtended()
        {
        }
    }
}
