using iChat.DTOs.Users.Messages;
using Microsoft.Extensions.Caching.Memory;

namespace iChat.BackEnd.Services.Users.Infra.MemoryCache
{
    public class MemCacheUserChatService
    {
        IMemoryCache _localCache;
        TimeSpan cacheTime = TimeSpan.FromMinutes(2);
        public MemCacheUserChatService(IMemoryCache memoryCache)
        {
            _localCache = memoryCache;
        }
        string cacheKey(string userId)
        {
            return $"user:{userId}:ServerList";
        }
        public  List<long> GetServerListAsync(string userId,bool ExtendExpire=false)
        {

            var key= cacheKey(userId);
            if (_localCache.TryGetValue(key, out List<long> serverList))
            {
                if (ExtendExpire)
                    _localCache.Set(key, serverList, new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = cacheTime
                    });
                return serverList;
            }
            return new();
        }
        public bool IsUserInServer(string userId, long serverId, bool extendExpire = false)
        {
            var serverList = GetServerListAsync(userId, extendExpire);
            return serverList.Contains(serverId);
        }
        public  void SetServerListAsync(long serverId, List<long> serverList)
        {
            var cacheKey = $"user:{serverId}:ServerList";
            _localCache.Set(cacheKey, serverList, cacheTime);
        }
        public  void SetServerListAsync(long serverId, List<ChatServerDto> serverList)
        {
             SetServerListAsync(serverId, serverList.Select(s =>long.Parse(s.Id)).ToList());
        }
    }
}
