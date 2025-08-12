using iChat.BackEnd.Models.ChatServer;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.Cache.ChatServer;
using iChat.DTOs.ChatServerDatas;
using iChat.DTOs.Collections;
using iChat.DTOs.Shared;
using iChat.DTOs.Users.Messages;
using System.Collections.Concurrent;

namespace iChat.BackEnd.Services.Users.Infra.MemoryCache.ChatServer
{
    public class MemCacheChatServerRepository : IChatServerRepository
    {
        private readonly ConcurrentDictionary<long, ChatServerData> _metadataMap = new();
        public bool UploadServersAsync(List<ChatServerbulk> servers)
        {
            foreach (var server in servers)
            {
                _metadataMap[server.Id] = new ChatServerData
                {
                    Id = new ServerId(new stringlong(server.Id)),
                    Name = server.Name,
                    AvatarUrl = server.AvatarUrl,
                    CreatedAt = server.CreatedAt,
                    Channels = server.Channels,
                    AdminId = new UserId(new stringlong(server.AdminId)),
                    Emojis = server.Emojis,
                };
            }
            return true;
        }

        public bool UploadNewServerAsync(ChatServerData server)
        {
            _metadataMap[server.Id.Value] = server;
            return true;
        }

        public OperationResultT<ChatServerData> GetServerAsync(ServerId serverId, bool isCopy = true)
        {
            if (!_metadataMap.TryGetValue(serverId.Value, out var cached) || cached is null)
                return OperationResultT<ChatServerData>.Fail("404", "Server metadata not found");

            ChatServerData result = !isCopy
                ? cached
                : new ChatServerData
                {
                    Id = cached.Id,
                    Name = cached.Name,
                    AvatarUrl = cached.AvatarUrl,
                    CreatedAt = cached.CreatedAt,
                    AdminId = cached.AdminId,
                    Channels = new List<ChatChannelDtoLite>(cached.Channels),
                    Emojis = cached.Emojis
                };

            return OperationResultT<ChatServerData>.Ok(result);
        }
        public  bool UpdateServerMetadata(ChatServerChangeUpdate update)
        {
            var server =  GetServerAsync(update.Id, false);
            if (!server.Success)
                throw new Exception("This server doesnt exist");
            if (!string.IsNullOrEmpty(update.Name))
                server.Value.Name = update.Name;
            if (!string.IsNullOrEmpty(update.AvatarUrl))
                server.Value.AvatarUrl = update.AvatarUrl;
            return true;
        }
    }
}
