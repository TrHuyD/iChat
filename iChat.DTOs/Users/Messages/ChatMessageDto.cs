using iChat.DTOs.Shared.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Messages
{
    public class ChatMessageComparer : IComparer<ChatMessageDto>
    {
        public int Compare(ChatMessageDto? x, ChatMessageDto? y)
        {
            if (x == null || y == null) return 0;
            return x.Id.CompareTo(y.Id);
        }
    }
    
    public class ChatMessageDto
    {
        public long Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public MediaFileDto? ContentMedia { get; set; }
        public int MessageType { get; set; }
     //   [JsonConverter(typeof(DateTimeOffsetJsonConverter))]
        public DateTimeOffset CreatedAt { get; set; }
        public long SenderId { get; set; }
        public long ChannelId { get; set; }
        public bool IsEdited { get; set; } 
        public bool IsDeleted { get; set; } 
        public ChatMessageDto()
        {
        }
        public ChatMessageDto(ChatMessageDtoSafe chatMessageDtoSafe)
        {
            Id = long.Parse(chatMessageDtoSafe.Id);
            Content = chatMessageDtoSafe.Content;
            ContentMedia = chatMessageDtoSafe.ContentMedia;
            MessageType = chatMessageDtoSafe.MessageType;
            CreatedAt = chatMessageDtoSafe.CreatedAt;
            SenderId =long.Parse( chatMessageDtoSafe.SenderId);
            ChannelId = long.Parse(chatMessageDtoSafe.ChannelId);
            IsEdited = chatMessageDtoSafe.isEdited;
            IsDeleted = chatMessageDtoSafe.isDeleted;
        }
        //     public string? SenderName { get; set; } = string.Empty;
        //   public string? SenderAvatarUrl { get; set; } = "https://cdn.discordapp.com/embed/avatars/0.png";
        public void HandleDeleteMedia()
        {
            if (ContentMedia == null)
                return;
        }
    }
}
