using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.Infra.Helpers;
using iChat.BackEnd.Services.Users.Infra.Neo4jService;
using iChat.BackEnd.Services.Users.Infra.Redis;
using iChat.BackEnd.Services.Users.Infra.Redis.Enums;
using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers
{
    public class ServerListService
    {

        RedisUserServerService _redisUserSerivce;
        ThreadSafeCacheService _lock;
        IChatListingService _chatListingService;
        public ServerListService(IChatListingService neo4jService, RedisUserServerService redisUserSerivce,ThreadSafeCacheService threadSafeCacheService)
        {
            _lock=threadSafeCacheService;
            _chatListingService = neo4jService;
            _redisUserSerivce = redisUserSerivce;
        }
        public async Task<List<ChatServerDto>> GetServerList(long userId)
        {
            return await _lock.GetOrAddAsync(
                getLockKey: () => RedisVariableKey.GetUserServerKey_Lock(userId),
                fetchFromCache: () => _redisUserSerivce.GetUserServersAsync(userId),
                fetchFromDb: () => _chatListingService.GetUserChatServersAsync(userId),
                saveToCache: servers => _redisUserSerivce.AddUserServersAsync(userId, servers));
        }
        public async Task<List<string>> GetChannelList(long serverId)
        {
        return await _lock.GetOrAddAsync(
            getLockKey: ()=>RedisVariableKey.GetServerChannelKey_Lock(serverId),
            fetchFromCache: () => _redisUserSerivce.GetServerChannelsAsync(serverId),
            fetchFromDb: () => _chatListingService.GetServerChannelListAsStringAsync(serverId),
            saveToCache: channels => _redisUserSerivce.AddServerChannelsAsync(serverId, channels));
        }
        public async Task<bool> CheckIfUserInServer(long userId, long serverId)
        {
            return await _lock.CheckAndFetchAsync(
                key: $"user:servers:{userId}",
                member: serverId,
                fetchFromCache: () => _redisUserSerivce.CheckIfUserInServer(userId, serverId),
                fetchFromDb: () => _chatListingService.GetUserChatServersAsync(userId),
                saveToCache: servers => _redisUserSerivce.AddUserServersAsync(userId, servers));
        }
    }
}
