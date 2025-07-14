using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.DTOs.Users;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace iChat.BackEnd.Services.Users.Infra.Memory.MessageServices
{
    public class UserMetadataMemoryCacheService : IUserMetaDataCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ConcurrentDictionary<string, long> _versions;
        private readonly TimeSpan _defaultTtl = TimeSpan.FromHours(1);
        private readonly TimeSpan _notFoundTtl = TimeSpan.FromMinutes(5);
        private string GetMetadataKey(string userId) => $"user:{userId}:metadata";
        private string GetVersionKey(string userId) => $"user:{userId}:version";
        private string GetPlaceholderKey(string userId) => $"user:{userId}:placeholder";
        public UserMetadataMemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _versions = new ConcurrentDictionary<string, long>();
        }
        public Task<UserMetadata?> GetAsync(string userId)
        {
            var metadataKey = GetMetadataKey(userId);
            var placeholderKey = GetPlaceholderKey(userId);

            // Check if placeholder exists (indicating not found)
            if (_memoryCache.TryGetValue(placeholderKey, out _))
            {
                return Task.FromResult<UserMetadata?>(null);
            }

            // Try to get actual metadata
            if (_memoryCache.TryGetValue(metadataKey, out UserMetadata? metadata))
            {
                return Task.FromResult(metadata);
            }

            return Task.FromResult<UserMetadata?>(null);
        }
        public Task<(Dictionary<string, UserMetadata> dic, List<string> missing)> GetManyAsync(List<string> userIds)
        {
            var result = new Dictionary<string, UserMetadata>();
            var missing = new List<string>();

            foreach (var userId in userIds)
            {
                var placeholderKey = GetPlaceholderKey(userId);

                // Skip if placeholder exists
                if (_memoryCache.TryGetValue(placeholderKey, out _))
                {
                    missing.Add(userId);
                    continue;
                }

                var metadataKey = GetMetadataKey(userId);
                if (_memoryCache.TryGetValue(metadataKey, out UserMetadata? metadata) && metadata != null)
                {
                    result[userId] = metadata;
                }
                else
                {
                    missing.Add(userId);
                }
            }

            return Task.FromResult((result, missing));
        }
        public Task SetAsync(UserMetadata user)
        {
            var metadataKey = GetMetadataKey(user.UserId);
            var placeholderKey = GetPlaceholderKey(user.UserId);

            // Remove placeholder if exists
            _memoryCache.Remove(placeholderKey);

            // Set metadata with expiration
            var entryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _defaultTtl,
                Priority = CacheItemPriority.Normal
            };

            _memoryCache.Set(metadataKey, user, entryOptions);

            // Update version
            IncrementVersionInternal(user.UserId);

            return Task.CompletedTask;
        }
        public Task SetManyAsync(IEnumerable<UserMetadata> users)
        {
            var userList = users.ToList();
            if (!userList.Any()) return Task.CompletedTask;

            var entryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _defaultTtl,
                Priority = CacheItemPriority.Normal
            };

            foreach (var user in userList)
            {
                var metadataKey = GetMetadataKey(user.UserId);
                var placeholderKey = GetPlaceholderKey(user.UserId);
                // Remove placeholder if exists
                _memoryCache.Remove(placeholderKey);
                // Set metadata
                _memoryCache.Set(metadataKey, user, entryOptions);
                // Update version
                IncrementVersionInternal(user.UserId);
            }
            return Task.CompletedTask;
        }

        public Task SetPlaceholderAsync(string userId)
        {
            var placeholderKey = GetPlaceholderKey(userId);
            var metadataKey = GetMetadataKey(userId);

            // Remove actual metadata if exists
            _memoryCache.Remove(metadataKey);

            // Set placeholder with shorter TTL
            var entryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _notFoundTtl,
                Priority = CacheItemPriority.Low
            };

            _memoryCache.Set(placeholderKey, true, entryOptions);

            return Task.CompletedTask;
        }

        public Task SetPlaceholdersAsync(IEnumerable<string> userIds)
        {
            var userIdList = userIds.ToList();
            if (!userIdList.Any()) return Task.CompletedTask;

            var entryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _notFoundTtl,
                Priority = CacheItemPriority.Low
            };

            foreach (var userId in userIdList)
            {
                var placeholderKey = GetPlaceholderKey(userId);
                var metadataKey = GetMetadataKey(userId);

                // Remove actual metadata if exists
                _memoryCache.Remove(metadataKey);

                // Set placeholder
                _memoryCache.Set(placeholderKey, true, entryOptions);
            }

            return Task.CompletedTask;
        }

        public Task<long> GetMetadataVersion(string userId)
        {
            var version = _versions.GetOrAdd(userId, 1);
            return Task.FromResult(version);
        }

        public Task ExpandExpire(string userId)
        {
            var metadataKey = GetMetadataKey(userId);
            var placeholderKey = GetPlaceholderKey(userId);

            // For metadata, we need to refresh the expiration
            if (_memoryCache.TryGetValue(metadataKey, out UserMetadata? metadata) && metadata != null)
            {
                var entryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _defaultTtl,
                    Priority = CacheItemPriority.Normal
                };
                _memoryCache.Set(metadataKey, metadata, entryOptions);
            }

            // For placeholder, refresh with shorter TTL
            if (_memoryCache.TryGetValue(placeholderKey, out _))
            {
                var entryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _notFoundTtl,
                    Priority = CacheItemPriority.Low
                };
                _memoryCache.Set(placeholderKey, true, entryOptions);
            }

            return Task.CompletedTask;
        }

        // Helper methods
        private void IncrementVersionInternal(string userId)
        {
            _versions.AddOrUpdate(userId, 1, (key, oldValue) => oldValue + 1);
        }

        public Task IncrementVersion(string userId)
        {
            IncrementVersionInternal(userId);
            return Task.CompletedTask;
        }

        public Task SetWithVersion(UserMetadata user)
        {
            SetAsync(user); // This already increments version
            return Task.CompletedTask;
        }

        // Additional cleanup method for memory management
        public void ClearExpiredEntries()
        {
            // MemoryCache handles this automatically, but we can clean up our version dictionary
            // This could be called periodically by a background service
            var expiredVersions = _versions.Where(kvp =>
                !_memoryCache.TryGetValue(GetMetadataKey(kvp.Key), out _) &&
                !_memoryCache.TryGetValue(GetPlaceholderKey(kvp.Key), out _))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var userId in expiredVersions)
            {
                _versions.TryRemove(userId, out _);
            }
        }

        // Get cache statistics (useful for monitoring)
        public (int MetadataCount, int PlaceholderCount, int VersionCount) GetCacheStatistics()
        {
            // Note: IMemoryCache doesn't expose count directly, this is an approximation
            return (0, 0, _versions.Count); // Actual counts would need reflection or custom tracking
        }
    }
}