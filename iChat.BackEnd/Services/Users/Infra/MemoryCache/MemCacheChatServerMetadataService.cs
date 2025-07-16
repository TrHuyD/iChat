using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.DTOs.Users.Messages;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using iChat.BackEnd.Collections;
using iChat.DTOs.Users;

namespace iChat.BackEnd.Services.Users.Infra.MemoryCache
{
    public class MemCacheChatServerMetadataService : IChatServerMetadataCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly MemCacheUserChatService _userChatService;

        private readonly Dictionary<long, ThreadSafeIndexedUserCollection> _serverOfflineUserMap = new();
        private readonly Dictionary<long, ThreadSafeIndexedUserCollection> _serverOnlineUserMap = new();

        public MemCacheChatServerMetadataService(IMemoryCache cache, MemCacheUserChatService userChatService)
        {
            _cache = cache;
            _userChatService = userChatService;
        }

        public Task<bool> UploadServersAsync(List<ChatServerMetadata> servers)
        {
            foreach (var server in servers)
            {
                var serverKey = GetServerKey(server.Id);
                _cache.Set(serverKey, server);

                if (server.Channels.Any())
                {
                    var channelMap = server.Channels.ToDictionary(c => c.Id);
                    _cache.Set(GetChannelKey(server.Id), channelMap);
                }
            }

            return Task.FromResult(true);
        }

        public void UploadServerAsync(ChatServerMetadata server)
        {
            var serverKey = GetServerKey(server.Id);
            _cache.Set(serverKey, server);

            if (server.Channels.Any())
            {
                var channelMap = server.Channels.ToDictionary(c => c.Id);
                _cache.Set(GetChannelKey(server.Id), channelMap);
            }
        }

        public Task<bool> IsAdmin(long serverId, long userId)
        {
            if (!_cache.TryGetValue(GetServerKey(serverId), out ChatServerMetadata? server))
                throw new KeyNotFoundException($"Server {serverId} not found in cache.");
            return Task.FromResult(server.AdminId == userId.ToString());
        }

        public Task<bool> IsAdmin(long serverId, long channelId, long userId)
        {
            if (!_cache.TryGetValue(GetServerKey(serverId), out ChatServerMetadata? server))
                throw new KeyNotFoundException($"Server {serverId} not found in cache.");

            var channel = channelId.ToString();
            if (!_userChatService.IsUserInServer(userId.ToString(), serverId))
                throw new KeyNotFoundException($"Member {userId} not in {serverId}");

            return Task.FromResult(server.AdminId == userId.ToString() && server.Channels.Any(c => c.Id == channel));
        }

        public Task<ChatServerMetadata?> GetServerAsync(string serverId, bool includeChannels = true)
        {
            if (!_cache.TryGetValue(GetServerKey(serverId), out ChatServerMetadata? server))
                return Task.FromResult<ChatServerMetadata?>(null);

            if (!includeChannels)
            {
                server.Channels = new List<ChatChannelDtoLite>();
                return Task.FromResult(server);
            }

            if (_cache.TryGetValue(GetChannelKey(serverId), out Dictionary<string, ChatChannelDtoLite>? channels))
            {
                server.Channels = channels.Values.OrderBy(c => c.Order).ToList();
            }

            return Task.FromResult(server);
        }

        public Task<long?> GetUserPermissionAsync(long userId, long serverId, long channelId)
        {
            var perms = GetOrCreateUserPerms(userId, serverId);
            return Task.FromResult(perms.TryGetValue(channelId, out var value) ? (long?)value : null);
        }

        public Task<bool> SetUserPermissionAsync(long userId, long serverId, long channelId, long perm)
        {
            var userPerms = GetOrCreateUserPerms(userId, serverId);
            userPerms[channelId] = perm;

            var channelPerms = GetOrCreateChannelPerms(serverId, channelId);
            channelPerms[userId] = perm;

            return Task.FromResult(true);
        }

        public Task<Dictionary<long, long>> GetAllUserPermsInServerAsync(long userId, long serverId)
        {
            return Task.FromResult(GetOrCreateUserPerms(userId, serverId));
        }

        public Task<Dictionary<long, long>> GetAllUserPermsInChannelAsync(long serverId, long channelId)
        {
            return Task.FromResult(GetOrCreateChannelPerms(serverId, channelId));
        }

        private Dictionary<long, long> GetOrCreateUserPerms(long userId, long serverId)
        {
            var key = $"user:{userId}:server:{serverId}:perms";
            return _cache.GetOrCreate(key, entry => new Dictionary<long, long>());
        }

        private Dictionary<long, long> GetOrCreateChannelPerms(long serverId, long channelId)
        {
            var key = $"server:{serverId}:channel:{channelId}:perms";
            return _cache.GetOrCreate(key, entry => new Dictionary<long, long>());
        }

        private string GetServerKey(long serverId) => $"server:{serverId}:meta";
        private string GetServerKey(string serverId) => $"server:{serverId}:meta";
        private string GetChannelKey(string serverId) => $"server:{serverId}:channels";

        public async Task IsInServerWithCorrectStruct(long userId, long serverId, long channelId)
        {
            if (!_cache.TryGetValue(GetServerKey(serverId), out ChatServerMetadata? server))
                throw new KeyNotFoundException($"Server {serverId} not found in cache.");

            var channel = channelId.ToString();
            if (!server.Channels.Any(c => c.Id == channel))
                throw new KeyNotFoundException($"Channel {channel} not found in server {serverId}.");

            if (!_userChatService.IsUserInServer(userId.ToString(), serverId))
                throw new KeyNotFoundException($"Member {userId} not in {serverId}");
        }

        public void AddChannelAsync(ChatChannelDto channel)
        {
            var serverId = channel.ServerId;
            if (!_cache.TryGetValue(GetServerKey(serverId), out ChatServerMetadata? server))
                throw new KeyNotFoundException($"Server {serverId} not found in cache.");

            server.Channels.Add(channel);
        }

        public void SetOnlineStatusForUserAcrossServers(long userId, List<long> serverIds)
        {
            foreach (var serverId in serverIds)
            {
                if (!_serverOnlineUserMap.TryGetValue(serverId, out var set))
                    _serverOnlineUserMap[serverId] = set = new ThreadSafeIndexedUserCollection();

                set.AddUser(userId);
            }
        }


        public List<long> GetOnlineUsersAsync(long serverId,int lim=50)
        {
            if(_serverOnlineUserMap.TryGetValue(serverId,out  var index))
            {
                return index.GetUserPage(0, lim);
            }
            throw new Exception($"Error while loading online user :Server havent loaded {serverId}");
        }

        public List<long> GetOfflineUsersAsync(long serverId, int lim = 50)
        {
            if (_serverOfflineUserMap.TryGetValue(serverId, out var index))
            {
                return index.GetUserPage(0, lim);
            }
            throw new Exception($"Error while loading offline user: Server havent loaded {serverId}");
        }

        public void SetAllUsers(long serverId, List<long> users)
        {
            if (!_serverOfflineUserMap.TryGetValue(serverId, out var set))
                _serverOfflineUserMap[serverId] = set = new ThreadSafeIndexedUserCollection();
            else
                set.Clear();

            set.AddRange(users);
        }

        public Task SetUserOnline(List<long> serverList, UserMetadata metadata, long userId = -1)
        {
            throw new NotImplementedException();
        }
    }
}
