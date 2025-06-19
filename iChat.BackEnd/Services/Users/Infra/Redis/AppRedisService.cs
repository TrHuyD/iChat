


using StackExchange.Redis;

namespace iChat.BackEnd.Services.Users.Infra.Redis
{
    public class AppRedisService
    {
        private readonly IDatabase _db;
        public AppRedisService(IRedisConnectionService connection) { _db = connection.GetDataBase(); }

        public async Task<string?> GetCacheValueAsync(string key) => await _db.StringGetAsync(key);
        public IDatabase GetDatabase() => _db;
        public async Task SetCacheValueAsync(string key, string value, TimeSpan expiry) =>
            await _db.StringSetAsync(key, value, expiry);

        public async Task<string?> GetCachedValueWithExpiryAsync(string key, Func<Task<string?>> action, TimeSpan expiry)
        {
            var cachedValue = await RetrieveAndExtendExpiryAsync(key, expiry);
            if (!string.IsNullOrEmpty(cachedValue)) return cachedValue;

            if (!await AcquireLockAsync(key))
            {
                await Task.Delay(1000);
                return await GetCacheValueAsync(key); // Retry fetch after waiting
            }

            try { return await ComputeAndCacheValueAsync(key, action, expiry); }
            finally { await ReleaseLockAsync(key); }
        }

        /// <summary> Retrieves cached value and extends expiry if it exists. </summary>
        public async Task<string?> RetrieveAndExtendExpiryAsync(string key, TimeSpan expiry)
        {
            var script = @"
                local value = redis.call('GET', KEYS[1])
                if value then redis.call('EXPIRE', KEYS[1], ARGV[1]) end
                return value";

            return (string?)await _db.ScriptEvaluateAsync(script, new RedisKey[] { key }, new RedisValue[] { (int)expiry.TotalSeconds });
        }

        /// <summary> Acquires a distributed lock to ensure only one process computes the value. </summary>
        private async Task<bool> AcquireLockAsync(string key)
        {
            var lockKey = $"{key}:lock";
            return await _db.StringSetAsync(lockKey, "1", TimeSpan.FromSeconds(5), When.NotExists);
        }

        /// <summary> Computes value using the provided function and stores it in Redis. </summary>
        private async Task<string?> ComputeAndCacheValueAsync(string key, Func<Task<string?>> action, TimeSpan expiry)
        {
            var result = await action();
            if (!string.IsNullOrEmpty(result)) await _db.StringSetAsync(key, result, expiry);
            return result;
        }

        /// <summary> Releases the distributed lock. </summary>
        private async Task ReleaseLockAsync(string key)
        {
            await _db.KeyDeleteAsync($"{key}:lock");
        }

        /// <summary> Checks if a member exists in a set and extends its expiry. </summary>
        public async Task<int> CheckAndExtendMembershipExpiryAsync(string setKey, long member, TimeSpan expiry)
        {
            var script = @"
                    -- Check if the list exists
                    if redis.call('EXISTS', KEYS[1]) == 0 then
                        return 2 -- List expired
                    end

                    -- Check if the user is in the list
                    if redis.call('SISMEMBER', KEYS[1], ARGV[1]) == 1 then
                        redis.call('EXPIRE', KEYS[1], ARGV[2])
                        return 1 -- User exists, expiry extended
                    end

                    return 0 -- List exists, but user is not in it
                ";

            var result = (long?)await _db.ScriptEvaluateAsync(script,
                new RedisKey[] { setKey.ToString() },
                new RedisValue[] { member, (int)expiry.TotalSeconds });

            return (int)(result ?? 2); 
        }


        /// <summary> Checks if a member exists in a Redis set. </summary>
        public async Task<bool> CheckMembershipAsync(string setKey, string member) =>
            await _db.SetContainsAsync(setKey, member);

        /// <summary> Computes and caches membership status in Redis. </summary>
        public async Task<bool> ComputeAndCacheMembershipAsync(string setKey, string member, Func<Task<bool>> action, TimeSpan expiry)
        {
            var result = await action();
            if (result)
            {
                await _db.SetAddAsync(setKey, member);
                await _db.KeyExpireAsync(setKey, expiry);
            }
            return result;
        }
        public async Task<List<string>> GetListAsync(string listKey)
        {
            string luaScript = @"
                            local listKey = KEYS[1]

                            -- Check if listKey exists
                            if redis.call('EXISTS', listKey) == 0 then
                                return nil
                            end

                            -- Return all members of the set
                            return redis.call('SMEMBERS', listKey)
                        ";

            RedisKey[] keys = { listKey };

            var result = await _db.ScriptEvaluateAsync(luaScript, keys);

            if (result.IsNull)
                return new List<string>(); // Return empty list if key is missing

            return ((RedisResult[])result).Select(r => (string)r!).ToList();
        }
        public async Task<bool> AddToList(string listKey, string member, TimeSpan expiry)
        {
            string luaScript = @"
                    local listKey = KEYS[1]
                    local member = ARGV[1]
                    local expiry = tonumber(ARGV[2])

                    -- Add member to the set
                    redis.call('SADD', listKey, member)

                    -- If the key was just created, set expiry
                    if redis.call('SCARD', listKey) == 1 then
                        redis.call('EXPIRE', listKey, expiry)
                    end

                    return true
                ";

                        RedisKey[] keys = { listKey };
                        RedisValue[] values = { member, (int)expiry.TotalSeconds };

                        var result = await _db.ScriptEvaluateAsync(luaScript, keys, values);
                        return (bool)result;
                    }

        public async Task<int> AddListAsync(string key, int expiryTime, IEnumerable<string> serverIds)
        {
            string luaScript = @"
                local userServerKey = KEYS[1]
                local expiryTime = ARGV[#ARGV] -- Last argument is expiry time
                table.remove(ARGV, #ARGV) -- Remove expiry time from ARGV

                -- Add all elements to the set
                redis.call('SADD', userServerKey, unpack(ARGV))

                -- Set expiration on the set
                redis.call('EXPIRE', userServerKey, expiryTime)

                -- Return total count
                return redis.call('SCARD', userServerKey)
                 ";

            RedisKey[] keys = { key };
            RedisValue[] values = serverIds.Select(id => (RedisValue)id).Append(expiryTime).ToArray();

            var result = await _db.ScriptEvaluateAsync(luaScript, keys, values);
            return (int)result!;
        }

        /// <summary> Releases a distributed lock for set memberships. </summary>
        private async Task ReleaseLockAsync(string setKey, string member)
        {
            await _db.KeyDeleteAsync($"{setKey}:{member}:lock");
        }
    }
}

