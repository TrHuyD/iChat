using iChat.DTOs.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.ChatServerDatas
{
    public class ChatServerMetadata
    {
        public ServerId Id { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;

    }
    public static class ChatServerMetadataExtension
    {
        public static ChatServerMetadata toMetadata(this ChatServerData data)
        {
            return new ChatServerMetadata
            {
                Id = data.Id,
                Name = data.Name,
                AvatarUrl = data.AvatarUrl,
            };
        }
    }
}
