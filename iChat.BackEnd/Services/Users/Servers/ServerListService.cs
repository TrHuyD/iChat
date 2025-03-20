using iChat.BackEnd.Services.Users.Infra.Helpers;
using iChat.BackEnd.Services.Users.Infra.Neo4jService;
using iChat.BackEnd.Services.Users.Infra.Redis;

namespace iChat.BackEnd.Services.Users.Servers
{
    public class ServerListService
    {
        Lazy<Neo4jChatListingService> _neo4jService;
        RedisUserServerService _redisUserSerivce;
        ThreadSafeCacheService _lock;
        public ServerListService(Lazy<Neo4jChatListingService> neo4jService, RedisUserServerService redisUserSerivce,ThreadSafeCacheService threadSafeCacheService)
        {
            _lock=threadSafeCacheService;
            _neo4jService = neo4jService;
            _redisUserSerivce = redisUserSerivce;
        }
        public async Task<List<string>> GetServerList(string userId)
        {
            return await _lock.GetOrAddAsync(
                key: $"user:servers:{userId}",
                fetchFromCache: () => _redisUserSerivce.GetUserServersAsync(userId),
                fetchFromDb: () => _neo4jService.Value.GetUserServersAsync(userId),
                saveToCache: servers => _redisUserSerivce.AddUserServersAsync(userId, servers));
        }
        public async Task<List<string>> GetChannelList(string serverId)
        {
        return await _lock.GetOrAddAsync(
            key: $"server:channels:{serverId}",
            fetchFromCache: () => _redisUserSerivce.GetServerChannelsAsync(serverId),
            fetchFromDb: () => _neo4jService.Value.GetServerChannelListAsync(serverId),
            saveToCache: channels => _redisUserSerivce.AddServerChannelsAsync(serverId, channels));
        }
        public async Task<bool> CheckIfUserInServer(string userId, string serverId)
        {
            return await _lock.CheckAndFetchAsync(
                key: $"user:servers:{userId}",
                member: serverId,
                fetchFromCache: () => _redisUserSerivce.CheckIfUserInServer(userId, serverId),
                fetchFromDb: () => _neo4jService.Value.GetUserServersAsync(userId),
                saveToCache: servers => _redisUserSerivce.AddUserServersAsync(userId, servers));
        }
    }
}
