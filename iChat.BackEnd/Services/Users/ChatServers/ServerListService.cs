using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.Infra.Helpers;
using iChat.BackEnd.Services.Users.Infra.MemoryCache;

using iChat.BackEnd.Services.Users.Infra.Redis;
using iChat.BackEnd.Services.Users.Infra.Redis.Enums;
using iChat.DTOs.Users.Messages;
using Microsoft.Extensions.Caching.Memory;

namespace iChat.BackEnd.Services.Users.ChatServers
{
    [Obsolete("This service is deprecated and will be removed in future versions. Use the new ChatServerService instead.")]
    public class ServerListService
    {

        RedisUserServerService _redisUserSerivce;
        ThreadSafeCacheService _lock;
        IChatListingService _chatListingService;
        MemCacheUserChatService _localCache;
        public ServerListService(IChatListingService _dbListingService,
            RedisUserServerService redisUserSerivce,
            ThreadSafeCacheService threadSafeCacheService,
            MemCacheUserChatService memoryCache)
        {
            _localCache = memoryCache;
            _lock =threadSafeCacheService;
            _chatListingService = _dbListingService;
            _redisUserSerivce = redisUserSerivce;
        }
        public async Task<List<ChatServerDtoUser>> GetServerList(long userId)
        {
            var result= await _chatListingService.GetUserChatServersAsync(userId);
           // _localCache.SetServerListAsync(userId, result);
            return result;
        }
        public async Task<List<string>> GetChannelList(long serverId)
        {
        var result= await _lock.GetOrAddAsync(
            getLockKey: ()=>RedisVariableKey.GetServerChannelKey_Lock(serverId),
            fetchFromCache: () => _redisUserSerivce.GetServerChannelsAsync(serverId),
            fetchFromDb: () => _chatListingService.GetServerChannelListAsStringAsync(serverId),
            saveToCache: channels => _redisUserSerivce.AddServerChannelsAsync(serverId, channels));
            return result;
        }
        //public async Task<bool> CheckIfUserInServer(long userId, long serverId)
        //{
        //    return await _lock.CheckAndFetchAsync(
        //        key: $"user:servers:{userId}",
        //        member: serverId,
        //        fetchFromCache: () => _redisUserSerivce.CheckIfUserInServer(userId, serverId),
        //        fetchFromDb: () => _chatListingService.GetUserChatServersAsync(userId),
        //        saveToCache: servers => _redisUserSerivce.AddUserServersAsync(userId, servers));
        //}
    }
}
