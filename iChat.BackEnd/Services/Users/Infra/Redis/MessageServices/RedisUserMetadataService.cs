using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.DTOs.Users;
using StackExchange.Redis;
using System.Text.Json;

namespace iChat.BackEnd.Services.Users.Infra.Redis.MessageServices
{
    public class UserMetadataRedisCacheService :IUserMetaDataCacheService
    {
        private readonly IDatabase _redis;
        private readonly TimeSpan _defaultTtl = TimeSpan.FromHours(1);
        private readonly TimeSpan _notFoundTtl = TimeSpan.FromMinutes(5);
        string GetMetadataKey(string userId) => $"user:{userId}:metadata";
        public UserMetadataRedisCacheService(AppRedisService appRedis)
        {
            _redis = appRedis.GetDatabase();
        }

        public async Task<UserMetadata?> GetAsync(string userId)
        {
            var key = GetMetadataKey(userId);
            var value = await _redis.StringGetAsync(key);
            if (value.IsNullOrEmpty) return null;

            return JsonSerializer.Deserialize<UserMetadata>(value!);
        }

        public async Task<(Dictionary<string, UserMetadata> dic, List<string> missing)> GetManyAsync(List<string> userIds)
        {
            var keys = userIds.Select(id=> (RedisKey)GetMetadataKey(id)).ToArray();
            var redisResults = await _redis.StringGetAsync(keys);
            List<string> missing = new List<string>();
            var result = new Dictionary<string, UserMetadata>();
            for (int i = 0; i < userIds.Count; i++)
            {
                if (redisResults[i].IsNullOrEmpty) continue;

                var data = JsonSerializer.Deserialize<UserMetadata>(redisResults[i]!);
                if (data != null)
                    result[userIds[i]] = data;
                else
                    missing.Add(userIds[i]);
            }
            return (result,missing);
        }

        public Task SetAsync(UserMetadata user)
        {
            var key = GetMetadataKey(user.UserId);
            var value = JsonSerializer.Serialize(user);
            return _redis.StringSetAsync(key, value, _defaultTtl);
        }
        public async Task SetManyAsync(IEnumerable<UserMetadata> users)
        {
            var entries = users
                .Select(u => new KeyValuePair<RedisKey, RedisValue>(
                    GetMetadataKey(u.UserId),
                    JsonSerializer.Serialize(u)
                ))
                .ToList();
            var batch = _redis.Multiplexer.GetDatabase().CreateBatch();

            var setTasks = new List<Task>();
            var expireTasks = new List<Task>();
            foreach (var entry in entries)
            {
                setTasks.Add(batch.StringSetAsync(entry.Key, entry.Value));
                expireTasks.Add(batch.KeyExpireAsync(entry.Key, _defaultTtl));
            }

            batch.Execute();

            await Task.WhenAll(setTasks.Concat(expireTasks));
        }

        public Task SetPlaceholderAsync(string userId)
        {
            return _redis.StringSetAsync(GetMetadataKey(userId), "", _notFoundTtl);
        }
        public Task SetPlaceholdersAsync(IEnumerable<string> userIds)
        {
            var tasks = userIds.Select(id => SetPlaceholderAsync(id));
            return Task.WhenAll(tasks);
        }
    }
}
