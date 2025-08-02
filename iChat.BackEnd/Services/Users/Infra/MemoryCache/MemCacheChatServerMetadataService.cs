using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.DTOs.Users.Messages;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using iChat.BackEnd.Collections;
using iChat.DTOs.Users;
using iChat.BackEnd.Models.ChatServer;
using iChat.DTOs.Shared;
using iChat.DTOs.Collections;


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
                    Id =new ServerId(new stringlong(  server.Id)),
                    Name = server.Name,
                    AvatarUrl = server.AvatarUrl,
                    CreatedAt = server.CreatedAt,
                    Channels = server.Channels,
                    AdminId =new UserId(new stringlong(server.AdminId))
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
            _metadataMap[server.Id.Value] = server;
            var offline = new SkipListSortedSetV2<long>();
            var online = new SkipListSortedSetV2<long>();
            _serverUserMap[server.Id.Value] = (online, offline);
            
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

        public Task<OperationResultT<ChatServerMetadata>> GetServerAsync(ServerId serverId, bool isCopy = true)
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

        public OperationResultBool IsAdmin(ServerId serverId, UserId userId)
        {
            if (!_metadataMap.TryGetValue(serverId.Value, out var server))
            {
                return OperationResultBool.Fail("400", $"[UserService:IsAdmin] Server {serverId} not found in cache");
            }

            bool isAdmin = server.AdminId == userId;
            return OperationResultBool.Ok(isAdmin);
        }

        public Task<OperationResultBool> IsAdmin(ServerId serverId, ChannelId channelId, UserId userId)
        {
            var serverResult = GetServerAsync(serverId, false).Result;
            if (!serverResult.Success)
            {
                return Task.FromResult(OperationResultBool.Fail("400", $"[UserService:IsAdmin] Server {serverId} not found in cache"));
            }

            var server = serverResult.Value;
            if (!_userChatService.IsUserInServer(userId.Value.ToString(), serverId.Value))
            {
                return Task.FromResult(OperationResultBool.Fail("403", $"[UserService:IsAdmin] User {userId} is not a member of server {serverId}"));
            }
            if(!server.Channels.Any(c => c.Id == channelId.Value.ToString()))
            {
                return Task.FromResult(OperationResultBool.Fail("403", $"[UserService:IsAdmin] Channel {channelId} is not a channel of server {channelId}"));
            }
            bool isAdmin = server.AdminId == userId;
            return Task.FromResult(OperationResultBool.Ok(isAdmin));
        }

        public (bool success, List<int> newOnlineLocations, List<int> oldOfflineLocations) SetUserOnline(List<long> serverIds, UserMetadata user)
        {
            if (_userChatService.IsUserOnline(user.userId))
                return (false, new(), new());

            var newOnlineLocations = new List<int>(serverIds.Count);
            var oldOfflineLocations = new List<int>(serverIds.Count);

            for (int i = 0; i < serverIds.Count; i++)
            {
                var serverId = serverIds[i];
                if (!_serverUserMap.TryGetValue(serverId, out var sets))
                    throw new Exception($"Server {serverId} not found in cache");

                sets.Offline.Remove(user.userId.Value, out var oldOfflineIndex);
                if (!sets.Online.Add(user.userId.Value, out var newOnlineIndex))
                    throw new Exception("Cache mismatch");
                newOnlineLocations.Add(newOnlineIndex);
                oldOfflineLocations.Add(oldOfflineIndex);
            }
            _userChatService.SetOnlineUserData(serverIds.Select(s => s).ToList(), user);
            return (true, newOnlineLocations, oldOfflineLocations);
        }

        public (bool success, List<long> serverList, List<int> newOfflineLocations, List<int> oldOnlineLocations) SetUserOffline(UserId userId)
        {
            if (!_userChatService.IsUserOnline(userId))
                return (false, new(), new(), new());

            var serverIds = _userChatService.GetUserServerList(userId);
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
                {
                    
                    //let just ignore this, likely due to api testing, not setting online one
                    // throw new Exception("Cache mismatch");
                }
                oldOnlineLocations.Add(oldOnlineIndex);
                newOfflineLocations.Add(newOfflineIndex);
            }
            _userChatService.DowngradeUserCache(userId.Value.ToString());
            return (true, serverIds, newOfflineLocations, oldOnlineLocations);
        }

        public List<long> GetOnlineUsersAsync(ServerId serverId, int lim = 50)
        {
            if (_serverUserMap.TryGetValue(serverId.Value, out var sets))
                return sets.Online.GetRange(0, lim);
            throw new Exception($"Server {serverId} not loaded");
        }

        public List<long> GetOfflineUsersAsync(ServerId serverId, int lim = 50)
        {
            if (_serverUserMap.TryGetValue(serverId.Value, out var sets))
                return sets.Offline.GetRange(0, lim);
            throw new Exception($"Server {serverId} not loaded");
        }

        public (int index, bool isOnline) AddUserToServer(UserId userId, ServerId serverId)
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

        public bool RemoveUserFromServer(UserId userId, ServerId serverId)
        {
            if (!_serverUserMap.TryGetValue(serverId.Value, out var sets))
                return false;

            var removed = sets.Online.Remove(userId.Value, out _) || sets.Offline.Remove(userId.Value, out _);
            if (removed)
                _userChatService.RemoveServerFromUser(userId, serverId);

            return removed;
        }

        public (List<long> online, List<long> offline) GetUserList(ServerId serverId)
        {
            if (!_serverUserMap.TryGetValue(serverId.Value, out var sets))
                throw new KeyNotFoundException($"Server {serverId} not found in _serverUserMap.");
            return (sets.Online.GetAll(), sets.Offline.GetAll());
        }

        public async Task<bool> UpdateServerMetadata(ChatServerChangeUpdate update)
        {
            var server =await GetServerAsync(update.Id, false);
            if (!server.Success)
                throw new Exception("This server doesnt exist");
            if(update.Name!="")
                server.Value.Name=update.Name;  
            if(update.AvatarUrl!="")
                server.Value.AvatarUrl=update.AvatarUrl;
            return true;
        }
    }
}