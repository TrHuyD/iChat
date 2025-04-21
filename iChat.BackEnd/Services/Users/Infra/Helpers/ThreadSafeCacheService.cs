using iChat.BackEnd.Services.Users.Infra.Redis;
using StackExchange.Redis;
using System.Text.Json.Serialization.Metadata;

namespace iChat.BackEnd.Services.Users.Infra.Helpers
{
    public class ThreadSafeCacheService
    {
        private readonly IDatabase _db;
        private readonly TimeSpan _lockExpiry = TimeSpan.FromSeconds(10);
        private readonly HashSet<string> _LocalLockKeys = new HashSet<string>(); 

        public ThreadSafeCacheService(AppRedisService redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task<bool> TryAcquireLockWithResultAsync(string key)
        {
            if (_LocalLockKeys.Contains(key))
                return false ;
            string lockValue = Guid.NewGuid().ToString();
            _LocalLockKeys.Add(key);
            var result= await _db.StringSetAsync(key, lockValue, _lockExpiry, When.NotExists);
            if (result == false)
            {
                _LocalLockKeys.Remove(key);
                return false;
            }
            return true;
        }

        public async Task ReleaseLockAsync(string key)
        {
            _LocalLockKeys.Remove(key);
            await _db.KeyDeleteAsync(key);
        }
        [Obsolete("GetOrAddAsync is deprecated, please use GetOrRenewWithLockAsync instead.")]
        public async Task<T?> GetOrAddAsync<T>(
            Func<string> getLockKey,
            Func<Task<T?>> fetchFromCache,
            Func<Task<T>> fetchFromDb,
            Func<T, Task> saveToCache)
        {
            var cachedData = await fetchFromCache();
            if (cachedData is not null)
            {
                return cachedData;
            }

            string key = getLockKey();
            if (await TryAcquireLockWithResultAsync(key))
            {
                try
                {
                    cachedData = await fetchFromCache();
                    if (cachedData is not null)
                    {
                        return cachedData;
                    }

                    cachedData = await fetchFromDb();
                    await saveToCache(cachedData);

                    return cachedData;
                }
                finally
                {
                    ReleaseLockAsync(key);
                }
            }

            await Task.Delay(300);
            return await fetchFromCache();
        }
        public async Task<T?> GetOrRenewWithLockAsync<T>(
            Func<Task<T>> fetchFromCache,
            Func<Task<T>> fetchFromDb,
            Func<T, Task> saveToCache,
            Func<string> getLockKey,
            Func<T?, bool> isCacheExpired,
            int maxRetry = 3,
            int delayMs = 250)
        {
            for (int attempt = 0; attempt < maxRetry; attempt++)
            {
                // Step 1: Try to get from cache first
                
                var cached = await fetchFromCache();
                if (cached is not null && !isCacheExpired(cached))
                {
                    return cached;
                }
                else
                {
                    // Retry again
                    cached = await fetchFromCache();
                    if (cached is not null && !isCacheExpired(cached))
                    {
                        return cached;
                    }
                }

                var lockKey = getLockKey();
                var lockAcquired = await TryAcquireLockWithResultAsync(lockKey);

                if (lockAcquired)
                {
                    try
                    {
                        // Step 1-1: Re-check cache after acquiring the lock
                        cached = await fetchFromCache();
                        if (cached is not null && !isCacheExpired(cached))
                        {
                            return cached;
                        }

                        // Step 1-2: Fetch from DB and update cache
                        var dbData = await fetchFromDb();
                        await saveToCache(dbData);

                        return dbData;
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"[Cache] Error during save : {ex.Message}");
                        throw;
                    }
                    finally
                    {
                        await ReleaseLockAsync(lockKey);
                    }
                }
                else
                {
                    Console.WriteLine($"[Cache] Lock {lockKey} not acquired (attempt {attempt + 1}/{maxRetry}). Waiting {delayMs}ms...");
                    await Task.Delay(delayMs);

                    // Step 2-1: Retry getting from cache again after wait
                    cached = await fetchFromCache();
                    if (cached is not null && !isCacheExpired(cached))
                    {
                        return cached;
                    }
                }
            }

            Console.Error.WriteLine("[Cache] Failed to get valid cache after retries.");
            return default;
        }

        public async Task<bool> CheckAndFetchAsync(
            string key,
            string member,
            Func<Task<int>> fetchFromCache,
            Func<Task<List<string>>> fetchFromDb,
            Func<List<string>, Task> saveToCache)
        {
            int cacheResult = await fetchFromCache();
            if (cacheResult == 1) return true;
            if (cacheResult == 0) return false;

            if (await TryAcquireLockWithResultAsync(key))
            {
                try
                {
                    cacheResult = await fetchFromCache();
                    if (cacheResult == 1) return true;
                    if (cacheResult == 0) return false;

                    var dbResult = await fetchFromDb();
                    await saveToCache(dbResult);

                    return dbResult.Contains(member);
                }
                finally
                {
                    ReleaseLockAsync(key);
                }
            }

            await Task.Delay(300);
            return await fetchFromCache() == 1;
        }
    }
}
