using iChat.DTOs.Users;
using iChat.DTOs.Users.Messages;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace iChat.BackEnd.Services.Users.Infra.MemoryCache
{
    public class MemCacheUserChatService
    {
        private readonly IMemoryCache _localCache;
        private readonly TimeSpan _combinedCacheTime = TimeSpan.FromMinutes(15);
        private readonly TimeSpan _metadataOnlyCacheTime = TimeSpan.FromMinutes(30);

        // Pool for reusing HashSet instances to reduce GC pressure
        private readonly ConcurrentBag<HashSet<long>> _hashSetPool = new();

        public MemCacheUserChatService(IMemoryCache memoryCache)
        {
            _localCache = memoryCache;
        }

        // Primary cache key for combined data (hot path)
        private string CombinedKey(string userId) => $"user:{userId}:combined";
        private string CombinedKey(long userId) => $"user:{userId}:combined";
        // Secondary cache key for metadata-only (for offline users)
        private string MetadataOnlyKey(string userId) => $"user:{userId}:metadata";

        private class CombinedUserData
        {
            public List<long>? ServerList { get; set; }
            public UserMetadata? Metadata { get; set; }
            public HashSet<long>? ServerSet { get; set; } // For O(1) lookups
        }

        #region Hot Path - Combined Operations (Most Frequent)

        /// <summary>
        /// Hot path: Gets both server membership and metadata version in one cache lookup
        /// Use this for message processing where you need both pieces of info
        /// </summary>
        public (bool isInServer, long? metadataVersion) CheckUserAccessAndVersion(string userId, long serverId, bool extendExpire = false)
        {
            var key = CombinedKey(userId);

            if (_localCache.TryGetValue(key, out CombinedUserData? data))
            {
                if (extendExpire)
                {
                    RefreshCacheEntry(key, data, _combinedCacheTime);
                }

                var isInServer = data.ServerSet?.Contains(serverId) == true;
                var version = data.Metadata?.Version;
                return (isInServer, version);
            }

            // Fallback to metadata-only cache if available
            var metadata = GetMetadataOnly(userId, extendExpire);
            return (false, metadata?.Version);
        }

        /// <summary>
        /// Hot path: Get all data needed for message processing in one lookup
        /// </summary>
        public MessageProcessingContext? GetMessageProcessingContext(string userId, long serverId, bool extendExpire = false)
        {
            var key = CombinedKey(userId);

            if (_localCache.TryGetValue(key, out CombinedUserData? data))
            {
                if (extendExpire)
                {
                    RefreshCacheEntry(key, data, _combinedCacheTime);
                }

                return new MessageProcessingContext
                {
                    IsInServer = data.ServerSet?.Contains(serverId) == true,
                    MetadataVersion = data.Metadata?.Version,
                    Metadata = data.Metadata,
                    UserServerList = data.ServerList
                };
            }

            // Fallback to metadata-only
            var metadata = GetMetadataOnly(userId, extendExpire);
            return new MessageProcessingContext
            {
                IsInServer = false,
                MetadataVersion = metadata?.Version,
                Metadata = metadata,
                UserServerList = null
            };
        }

        #endregion

        #region Online User Operations (Combined Data)

        /// <summary>
        /// Use when user comes online - cache both server list and metadata together
        /// </summary>
        public void SetOnlineUserData(string userId, List<long> serverList, UserMetadata metadata)
        {
            var key = CombinedKey(userId);
            var serverSet = GetOrCreateHashSet();

            // Populate the HashSet efficiently
            foreach (var serverId in serverList)
            {
                serverSet.Add(serverId);
            }

            var data = new CombinedUserData
            {
                ServerList = serverList,
                Metadata = metadata,
                ServerSet = serverSet
            };

            SetCacheEntry(key, data, _combinedCacheTime);

            // Remove metadata-only cache since we have combined data now
            _localCache.Remove(MetadataOnlyKey(userId));
        }

        /// <summary>
        /// Update server list for online user - use when user joins/leaves servers
        /// </summary>
        public bool UpdateOnlineUserServers(string userId, List<long> serverList)
        {
            var key = CombinedKey(userId);

            if (_localCache.TryGetValue(key, out CombinedUserData? data))
            {
                // Return old HashSet to pool and create new one
                if (data.ServerSet != null)
                {
                    ReturnHashSetToPool(data.ServerSet);
                }

                data.ServerList = serverList;
                data.ServerSet = GetOrCreateHashSet();

                foreach (var serverId in serverList)
                {
                    data.ServerSet.Add(serverId);
                }

                SetCacheEntry(key, data, _combinedCacheTime);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Add a server to user's server list (when user joins a new server)
        /// </summary>
        public bool AddServerToUser(long userId, long serverId)
        {
            var key = CombinedKey(userId);

            if (_localCache.TryGetValue(key, out CombinedUserData? data))
            {
                if (data.ServerList != null && data.ServerSet != null)
                {
                    if (!data.ServerSet.Contains(serverId))
                    {
                        data.ServerList.Add(serverId);
                        data.ServerSet.Add(serverId);

                        SetCacheEntry(key, data, _combinedCacheTime);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Remove a server from user's server list (when user leaves a server)
        /// </summary>
        public bool RemoveServerFromUser(string userId, long serverId)
        {
            var key = CombinedKey(userId);

            if (_localCache.TryGetValue(key, out CombinedUserData? data))
            {
                if (data.ServerList != null && data.ServerSet != null)
                {
                    if (data.ServerSet.Contains(serverId))
                    {
                        data.ServerList.Remove(serverId);
                        data.ServerSet.Remove(serverId);

                        SetCacheEntry(key, data, _combinedCacheTime);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Update metadata for online user
        /// </summary>
        public bool UpdateOnlineUserMetadata(string userId, UserMetadata metadata)
        {
            var key = CombinedKey(userId);

            if (_localCache.TryGetValue(key, out CombinedUserData? data))
            {
                data.Metadata = metadata;
                SetCacheEntry(key, data, _combinedCacheTime);
                return true;
            }
            return false;
        }

        #endregion

        #region Offline User Operations (Metadata Only)

        /// <summary>
        /// Use for offline users - cache only metadata with longer TTL
        /// </summary>
        public void SetOfflineUserMetadata(string userId, UserMetadata metadata)
        {
            // Only set if we don't have combined data
            if (!_localCache.TryGetValue(CombinedKey(userId), out _))
            {
                var key = MetadataOnlyKey(userId);
                _localCache.Set(key, metadata, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = _metadataOnlyCacheTime,
                    Size = 1
                });
            }
        }

        /// <summary>
        /// Get metadata for offline user
        /// </summary>
        public UserMetadata? GetMetadataOnly(string userId, bool extendExpire = false)
        {
            var key = MetadataOnlyKey(userId);
            if (_localCache.TryGetValue(key, out UserMetadata? metadata))
            {
                if (extendExpire)
                {
                    _localCache.Set(key, metadata, new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = _metadataOnlyCacheTime,
                        Size = 1
                    });
                }
                return metadata;
            }
            return null;
        }

        #endregion

        #region Individual Operations (For Specific Cases)

        public bool IsUserInServer(string userId, long serverId, bool extendExpire = false)
        {
            var (isInServer, _) = CheckUserAccessAndVersion(userId, serverId, extendExpire);
            return isInServer;
        }

        public long? GetMetadataVersion(string userId, bool extendExpire = false)
        {
            var (_, version) = CheckUserAccessAndVersion(userId, 0, extendExpire);
            return version;
        }

        public UserMetadata? GetUserMetadata(string userId, bool extendExpire = false)
        {
            // Check combined cache first
            var key = CombinedKey(userId);
            if (_localCache.TryGetValue(key, out CombinedUserData? data))
            {
                if (extendExpire)
                {
                    RefreshCacheEntry(key, data, _combinedCacheTime);
                }
                return data.Metadata;
            }

            // Fallback to metadata-only cache
            return GetMetadataOnly(userId, extendExpire);
        }

        /// <summary>
        /// Get user's server list (only available for online users)
        /// </summary>
        public List<long>? GetUserServerList(string userId, bool extendExpire = false)
        {
            var key = CombinedKey(userId);
            if (_localCache.TryGetValue(key, out CombinedUserData? data))
            {
                return data.ServerList;
            }
            return null; // Server list is only available for online users
        }

        /// <summary>
        /// Get all servers user is in as a HashSet for fast lookups (only available for online users)
        /// </summary>
        public HashSet<long>? GetUserServerSet(string userId, bool extendExpire = false)
        {
            var key = CombinedKey(userId);
            if (_localCache.TryGetValue(key, out CombinedUserData? data))
            {
                return data.ServerSet;
            }
            return null; // Server set is only available for online users
        }

        /// <summary>
        /// Get count of servers user is in (only available for online users)
        /// </summary>
        public int GetUserServerCount(string userId, bool extendExpire = false)
        {
            var key = CombinedKey(userId);
            if (_localCache.TryGetValue(key, out CombinedUserData? data))
            {
                return data.ServerList?.Count ?? 0;
            }
            return 0; // Server count is only available for online users
        }

        /// <summary>
        /// Check if user is currently online (has combined cache entry)
        /// </summary>
        public bool IsUserOnline(string userId)
        {
            return _localCache.TryGetValue(CombinedKey(userId), out _);
        }

        /// <summary>
        /// Check if user exists in cache (either online or offline)
        /// </summary>
        public bool IsUserCached(string userId)
        {
            return _localCache.TryGetValue(CombinedKey(userId), out _) ||
                   _localCache.TryGetValue(MetadataOnlyKey(userId), out _);
        }

        #endregion

        #region Cache Management

        public void InvalidateUser(string userId)
        {
            var combinedKey = CombinedKey(userId);

            // Return HashSet to pool before removing
            if (_localCache.TryGetValue(combinedKey, out CombinedUserData? data))
            {
                if (data.ServerSet != null)
                {
                    ReturnHashSetToPool(data.ServerSet);
                }
            }

            _localCache.Remove(combinedKey);
            _localCache.Remove(MetadataOnlyKey(userId));
        }

        /// <summary>
        /// Downgrade user's cache when they go offline - move from combined cache to metadata-only cache
        /// This saves memory by removing server list data while keeping metadata accessible
        /// </summary>
        public void DowngradeUserCache(string userId)
        {
            var key = CombinedKey(userId);
            if (_localCache.TryGetValue(key, out CombinedUserData? data) && data.Metadata != null)
            {
                // Move metadata to offline cache with longer TTL
                SetOfflineUserMetadata(userId, data.Metadata);
                // Return HashSet to pool
                if (data.ServerSet != null)
                {
                    ReturnHashSetToPool(data.ServerSet);
                }
            }

            // Remove combined cache to free memory
            _localCache.Remove(key);
        }

        /// <summary>
        /// Alias for DowngradeUserCache - use when user goes offline
        /// </summary>
        public void MoveUserToOffline(string userId) => DowngradeUserCache(userId);

        /// <summary>
        /// Batch invalidate multiple users efficiently
        /// </summary>
        public void InvalidateUsers(IEnumerable<string> userIds)
        {
            foreach (var userId in userIds)
            {
                InvalidateUser(userId);
            }
        }

        #endregion

        #region Backward Compatibility

        public void SetUserChatData(string userId, List<long>? serverList = null, UserMetadata? metadata = null)
        {
            if (serverList != null && metadata != null)
            {
                SetOnlineUserData(userId, serverList, metadata);
            }
            else if (metadata != null)
            {
                SetOfflineUserMetadata(userId, metadata);
            }
            else if (serverList != null)
            {
                UpdateOnlineUserServers(userId, serverList);
            }
        }

        public (List<long>? serverList, UserMetadata? metadata) GetUserChatData(string userId, bool extendExpire = false)
        {
            var key = CombinedKey(userId);
            if (_localCache.TryGetValue(key, out CombinedUserData? data))
            {
                return (data.ServerList, data.Metadata);
            }

            var metadata = GetMetadataOnly(userId, extendExpire);
            return (null, metadata);
        }

        #endregion

        #region Helper Methods

        private void SetCacheEntry(string key, CombinedUserData data, TimeSpan expiration)
        {
            _localCache.Set(key, data, new MemoryCacheEntryOptions
            {
                SlidingExpiration = expiration,
                Size = CalculateSize(data)
            });
        }

        private void RefreshCacheEntry(string key, CombinedUserData data, TimeSpan expiration)
        {
            _localCache.Set(key, data, new MemoryCacheEntryOptions
            {
                SlidingExpiration = expiration,
                Size = CalculateSize(data)
            });
        }

        private HashSet<long> GetOrCreateHashSet()
        {
            if (_hashSetPool.TryTake(out var hashSet))
            {
                hashSet.Clear();
                return hashSet;
            }
            return new HashSet<long>();
        }

        private void ReturnHashSetToPool(HashSet<long> hashSet)
        {
            if (hashSet.Count < 1000) // Don't pool very large sets
            {
                hashSet.Clear();
                _hashSetPool.Add(hashSet);
            }
        }

        private int CalculateSize(CombinedUserData data)
        {
            var size = 2; // Base size for metadata
            if (data.ServerList != null)
            {
                // More accurate size calculation
                size += Math.Max(1, data.ServerList.Count / 8); // 8 longs per size unit
            }
            if (data.ServerSet != null)
            {
                size += Math.Max(1, data.ServerSet.Count / 8); // HashSet overhead
            }
            return size;
        }

        #endregion
    }



}
