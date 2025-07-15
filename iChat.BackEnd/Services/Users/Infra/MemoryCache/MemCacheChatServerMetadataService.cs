using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.DTOs.Users.Messages;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Channels;

namespace iChat.BackEnd.Services.Users.Infra.MemoryCache
{
    public class MemCacheChatServerMetadataService : IChatServerMetadataCacheService
    {
        private readonly IMemoryCache _cache;
        private MemCacheUserChatService _userChatService ;
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
                    var channelMap = new Dictionary<string, ChatChannelDtoLite>();
                    foreach (var channel in server.Channels)
                    {
                        channelMap[channel.Id] = channel;
                    }
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
                var channelMap = new Dictionary<string, ChatChannelDtoLite>();
                foreach (var channel in server.Channels)
                {
                    channelMap[channel.Id] = channel;
                }
                _cache.Set(GetChannelKey(server.Id), channelMap);
            }
           
        }
        public Task<bool> IsAdmin(long serverId,long userId)
        {
            if (!_cache.TryGetValue(GetServerKey(serverId), out ChatServerMetadata? server))
                throw new KeyNotFoundException($"Server {serverId} not found in cache.");
            return Task.FromResult(server.AdminId == userId.ToString());
        }
        public Task<bool> IsAdmin(long ServerId,long ChannelId,long UserId)
        {
            if (!_cache.TryGetValue(GetServerKey(ServerId), out ChatServerMetadata? server))
                throw new KeyNotFoundException($"Server {ServerId} not found in cache.");
            var channel = ChannelId.ToString();
            if (!_userChatService.IsUserInServer(UserId.ToString(), ServerId))
                throw new KeyNotFoundException($"Member {UserId} not in {ServerId}");
            return Task.FromResult(server.AdminId == UserId.ToString()&&server.Channels.Any(c=>c.Id==channel));
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
            var perms = GetOrCreateUserPerms(userId, serverId);
            return Task.FromResult(perms);
        }

        public Task<Dictionary<long, long>> GetAllUserPermsInChannelAsync(long serverId, long channelId)
        {
            var perms = GetOrCreateChannelPerms(serverId, channelId);
            return Task.FromResult(perms);
        }

        // Helpers

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
        private string GetChannelKey(long serverId) => $"server:{serverId}:channels";
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
    }
}
