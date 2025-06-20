using iChat.DTOs.Users.Messages;
using Microsoft.Extensions.Caching.Memory;

namespace iChat.BackEnd.Services.Users.Infra.MemoryCache
{
    public class MemCacheUserChatService
    {
        IMemoryCache _localCache;
        public MemCacheUserChatService(IMemoryCache memoryCache)
        {
            _localCache = memoryCache;
        }
        public  List<string> GetServerListAsync(long userId)
        {
            if (_localCache.TryGetValue($"user:{userId}:ServerList", out List<string> serverList))
            {
                return serverList;
            }
            return new();
        }
        public  void SetServerListAsync(long serverId, List<string> serverList)
        {
            var cacheKey = $"user:{serverId}:ServerList";
            _localCache.Set(cacheKey, serverList, TimeSpan.FromMinutes(1));
        }
        public  void SetServerListAsync(long serverId, List<ChatServerDto> serverList)
        {
             SetServerListAsync(serverId, serverList.Select(s => s.Id.ToString()).ToList());
        }
    }
}
