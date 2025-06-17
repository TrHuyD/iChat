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
        public string ContentMedia { get; set; } = string.Empty;
        public int MessageType { get; set; }
     //   [JsonConverter(typeof(DateTimeOffsetJsonConverter))]
        public DateTimeOffset CreatedAt { get; set; }
        public long SenderId { get; set; }
        public long RoomId { get; set; }
        //     public string? SenderName { get; set; } = string.Empty;
        //   public string? SenderAvatarUrl { get; set; } = "https://cdn.discordapp.com/embed/avatars/0.png";

    }
}
