using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace iChat.BackEnd.Services.Users.Infra.MemoryCache
{
    public class MemCacheUserPresence : IUserPresenceCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<MemCacheUserPresence> _logger;
        private readonly TimeSpan _presenceTimeout = TimeSpan.FromSeconds(60);
        private readonly ConcurrentDictionary<long, PresenceSet> _onlineUsersByServer = new();
        public MemCacheUserPresence(IMemoryCache cache, ILogger<MemCacheUserPresence> logger)
        {
            _cache = cache;
            _logger = logger;
        }
        public List<string> GetOnlineUsersPaged(long serverId, int page = 0, int pageSize = 50)
        {
            return _onlineUsersByServer.TryGetValue(serverId, out var set) ? set.GetPage(page, pageSize): new List<string>();
        }

        public void RemoveUserFromServer(string userId, long serverId)
        {
            if (_onlineUsersByServer.TryGetValue(serverId, out var set))
            {
                if (set.Remove(userId))
                {
                    _logger.LogDebug("User {UserId} removed from server {ServerId}", userId, serverId);
                }

                if (set.IsEmpty)
                    _onlineUsersByServer.TryRemove(serverId, out _);
            }
        }

        public void SetUserOffline(string userId, IEnumerable<long> serverIds)
        {

            foreach (var serverId in serverIds)
            {
                RemoveUserFromServer(userId, serverId);
                _cache.Remove(GetCacheKey(userId, serverId));
            }
        }

        public void SetUserOnline(string userId, IEnumerable<long> serverIds)
        {
            foreach (var serverId in serverIds)
            {
                var presenceSet = _onlineUsersByServer.GetOrAdd(serverId, _ => new PresenceSet());
 

                var cacheKey = GetCacheKey(userId, serverId);
                _cache.Set(cacheKey, true, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = _presenceTimeout,
                    PostEvictionCallbacks =
                {
                    new PostEvictionCallbackRegistration
                    {
                        EvictionCallback = (key, value, reason, state) =>
                        {
                            if (key is string k && TryParseCacheKey(k, out var uid, out var sid))
                            {
                                RemoveUserFromServer(uid, sid);
                            }
                        }
                    }
                }
                });
            }
        }
        protected static string GetCacheKey(string userId, long serverId) => $"presence:{serverId}:{userId}";
        protected static bool TryParseCacheKey(string key, out string userId, out long serverId)
        {
            var parts = key.Split(':');
            if (parts.Length == 3 && parts[0] == "presence")
            {
                serverId = long.Parse(parts[1]);
                userId = parts[2];
                return true;
            }
            userId = null;
            serverId = 0;
            return false;
        }
    }
    internal class PresenceSet
    {
        private readonly ConcurrentDictionary<string, byte> _userSet = new();
        private readonly List<string> _userList = new();
        private readonly object _lock = new();

        public bool Add(string userId)
        {
            if (_userSet.TryAdd(userId, 0))
            {
                lock (_lock)
                {
                    _userList.Add(userId);
                }
                return true;
            }
            return false;
        }

        public bool Remove(string userId)
        {
            if (_userSet.TryRemove(userId, out _))
            {
                lock (_lock)
                {
                    _userList.Remove(userId);
                }
                return true;
            }
            return false;
        }

        public List<string> GetAll()
        {
            lock (_lock)
            {
                return new List<string>(_userList);
            }
        }

        public List<string> GetPage(int page, int pageSize)
        {
            lock (_lock)
            {
                return _userList
                    .Skip(page * pageSize)
                    .Take(pageSize)
                    .ToList();
            }
        }

        public bool IsEmpty => _userSet.IsEmpty;
    }
}
