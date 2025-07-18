using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.DTOs.Users.Messages;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using iChat.BackEnd.Collections;
using iChat.DTOs.Users;
using iChat.BackEnd.Models.ChatServer;
using iChat.DTOs.Shared;


namespace iChat.BackEnd.Services.Users.Infra.MemoryCache
{
    public class MemCacheChatServerMetadataService : IChatServerMetadataCacheService
    {
        private readonly ConcurrentDictionary<long, ChatServerMetadata> _metadataMap = new();
        private readonly MemCacheUserChatService _userChatService;

        private readonly ConcurrentDictionary<long, (SkipListSortedSetV2<long> Online, SkipListSortedSetV2<long> Offline)> _serverUserMap = new();

        public MemCacheChatServerMetadataService(MemCacheUserChatService userChatService)
        {
            _userChatService = userChatService;
        }

        public Task<bool> UploadServersAsync(List<ChatServerbulk> servers)
        {
            foreach (var server in servers)
            {
                _metadataMap[server.Id] = new ChatServerMetadata
                {
                    Id = server.Id.ToString(),
                    Name = server.Name,
                    AvatarUrl = server.AvatarUrl,
                    CreatedAt = server.CreatedAt,
                    Channels = server.Channels,
                    AdminId = server.AdminId.ToString()
                };
                var offline = new SkipListSortedSetV2<long>();
                offline.Initialize(server.memberList);
                var online = new SkipListSortedSetV2<long>();
                _serverUserMap[server.Id] = (online, offline);
            }
            return Task.FromResult(true);
        }
        public Task<bool> UploadNewServerAsync(ChatServerMetadata server)
        {
            var serverId = long.Parse(server.Id);
            _metadataMap[serverId] = server;
            var offline = new SkipListSortedSetV2<long>();
            var online = new SkipListSortedSetV2<long>();
            _serverUserMap[serverId] = (online, offline);
            
            return Task.FromResult(true);
        }
        public Task<bool> AddChannel(ChatChannelDto channel)
        {
            var metadata = GetServerAsync(channel.ServerId, false);
            if (!metadata.Result.Success)
                return Task.FromResult(false);
            metadata.Result.Value.Channels.Add(channel);
            return Task.FromResult(true);   
        }
        public Task<OperationResultT<ChatServerMetadata>> EditServerProfile(stringlong serverId, stringlong requestorId, string newName, string avatarUrl = "")
        {
            var metadata = GetServerAsync(serverId, false).Result.Value;
            if (metadata == null)
                return Task.FromResult(OperationResultT<ChatServerMetadata>.Fail("404", "Server not found"));

            if (requestorId.StringValue != metadata.AdminId)
                return Task.FromResult(OperationResultT<ChatServerMetadata>.Fail("403", "You are not an admin"));

            metadata.Name = newName;
            metadata.AvatarUrl = avatarUrl;
            return Task.FromResult(OperationResultT<ChatServerMetadata>.Ok(metadata));
        }

        public Task<OperationResultT<ChatServerMetadata>> GetServerAsync(stringlong serverId, bool isCopy = true)
        {
            if (!_metadataMap.TryGetValue(serverId.Value, out var cached) || cached is null)
                return Task.FromResult(OperationResultT<ChatServerMetadata>.Fail("404", "Server metadata not found"));

            ChatServerMetadata result = !isCopy
                ? cached
                : new ChatServerMetadata
                {
                    Id = cached.Id,
                    Name = cached.Name,
                    AvatarUrl = cached.AvatarUrl,
                    CreatedAt = cached.CreatedAt,
                    AdminId = cached.AdminId,
                    Channels = new List<ChatChannelDtoLite>(cached.Channels)
                };

            return Task.FromResult(OperationResultT<ChatServerMetadata>.Ok(result));
        }

        public OperationResultBool IsAdmin(stringlong serverId, stringlong userId)
        {
            if (!_metadataMap.TryGetValue(serverId.Value, out var server))
            {
                return OperationResultBool.Fail("400", $"[UserService:IsAdmin] Server {serverId} not found in cache");
            }

            bool isAdmin = server.AdminId == userId.StringValue;
            return OperationResultBool.Ok(isAdmin);
        }

        public Task<OperationResultBool> IsAdmin(stringlong serverId, stringlong channelId, stringlong userId)
        {
            var serverResult = GetServerAsync(serverId, false).Result;
            if (!serverResult.Success)
            {
                return Task.FromResult(OperationResultBool.Fail("400", $"[UserService:IsAdmin] Server {serverId} not found in cache"));
            }

            var server = serverResult.Value;
            if (!_userChatService.IsUserInServer(userId.StringValue, serverId.Value))
            {
                return Task.FromResult(OperationResultBool.Fail("403", $"[UserService:IsAdmin] User {userId} is not a member of server {serverId}"));
            }
            if(!server.Channels.Any(c => c.Id == channelId.StringValue))
            {
                return Task.FromResult(OperationResultBool.Fail("403", $"[UserService:IsAdmin] Channel {channelId} is not a channel of server {channelId}"));
            }
            bool isAdmin = server.AdminId == userId.StringValue;
            return Task.FromResult(OperationResultBool.Ok(isAdmin));
        }

        public (bool success, List<int> newOnlineLocations, List<int> oldOfflineLocations) SetUserOnline(List<long> serverIds, UserMetadata user)
        {
            var userId = long.Parse(user.UserId);
            if (_userChatService.IsUserOnline(user.UserId))
                return (false, new(), new());

            var newOnlineLocations = new List<int>(serverIds.Count);
            var oldOfflineLocations = new List<int>(serverIds.Count);

            for (int i = 0; i < serverIds.Count; i++)
            {
                var serverId = serverIds[i];
                if (!_serverUserMap.TryGetValue(serverId, out var sets))
                    throw new Exception($"Server {serverId} not found in cache");

                sets.Offline.Remove(userId, out var oldOfflineIndex);
                if (!sets.Online.Add(userId, out var newOnlineIndex))
                    throw new Exception("Cache mismatch");
                newOnlineLocations.Add(newOnlineIndex);
                oldOfflineLocations.Add(oldOfflineIndex);
            }
            _userChatService.SetOnlineUserData(serverIds.Select(s => s).ToList(), user);
            return (true, newOnlineLocations, oldOfflineLocations);
        }

        public (bool success, List<long> serverList, List<int> newOfflineLocations, List<int> oldOnlineLocations) SetUserOffline(stringlong userId)
        {
            if (!_userChatService.IsUserOnline(userId.StringValue))
                return (false, new(), new(), new());

            var serverIds = _userChatService.GetUserServerList(userId.StringValue);
            var newOfflineLocations = new List<int>(serverIds.Count);
            var oldOnlineLocations = new List<int>(serverIds.Count);

            for (int i = 0; i < serverIds.Count; i++)
            {
                var serverId = serverIds[i];
                if (!_serverUserMap.TryGetValue(serverId, out var sets))
                    throw new Exception($"Server {serverId} not found in cache");

                if (!sets.Online.Remove(userId.Value, out var oldOnlineIndex))
                    throw new Exception($"Cache mismatch removing user {userId} from online");

                if (!sets.Offline.Add(userId.Value, out var newOfflineIndex))
                    throw new Exception("Cache mismatch");

                oldOnlineLocations.Add(oldOnlineIndex);
                newOfflineLocations.Add(newOfflineIndex);
            }
            _userChatService.DowngradeUserCache(userId.StringValue);
            return (true, serverIds, newOfflineLocations, oldOnlineLocations);
        }

        public List<long> GetOnlineUsersAsync(stringlong serverId, int lim = 50)
        {
            if (_serverUserMap.TryGetValue(serverId.Value, out var sets))
                return sets.Online.GetRange(0, lim);
            throw new Exception($"Server {serverId} not loaded");
        }

        public List<long> GetOfflineUsersAsync(stringlong serverId, int lim = 50)
        {
            if (_serverUserMap.TryGetValue(serverId.Value, out var sets))
                return sets.Offline.GetRange(0, lim);
            throw new Exception($"Server {serverId} not loaded");
        }

        public (int index, bool isOnline) AddUserToServer(stringlong userId, stringlong serverId)
        {
            var online = _userChatService.AddServerToUser(userId.Value, serverId.Value);
            return (_AddUserToServer(userId.Value, serverId.Value, online), online);
        }

        private int _AddUserToServer(long userId, long serverId, bool online)
        {
            if (!_serverUserMap.TryGetValue(serverId, out var sets))
                throw new KeyNotFoundException($"Server {serverId} not found in _serverUserMap.");
            int index=-1;
            if (online)
                sets.Online.Add(userId, out index);
            else
                sets.Offline.Add(userId, out index);
            return index;
        }

        public bool RemoveUserFromServer(stringlong userId, stringlong serverId)
        {
            if (!_serverUserMap.TryGetValue(serverId.Value, out var sets))
                return false;

            var removed = sets.Online.Remove(userId.Value, out _) || sets.Offline.Remove(userId.Value, out _);
            if (removed)
                _userChatService.RemoveServerFromUser(userId.StringValue, serverId.Value);

            return removed;
        }

        public (List<long> online, List<long> offline) GetUserList(stringlong serverId)
        {
            if (!_serverUserMap.TryGetValue(serverId.Value, out var sets))
                throw new KeyNotFoundException($"Server {serverId} not found in _serverUserMap.");
            return (sets.Online.GetAll(), sets.Offline.GetAll());
        }
    }
}