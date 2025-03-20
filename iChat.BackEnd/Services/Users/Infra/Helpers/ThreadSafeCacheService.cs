using System.Collections.Concurrent;

namespace iChat.BackEnd.Services.Users.Infra.Helpers
{
    public class ThreadSafeCacheService
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        public async Task<T> GetOrAddAsync<T>(
            string key,
            Func<Task<T?>> fetchFromCache,
            Func<Task<T>> fetchFromDb,
            Func<T, Task> saveToCache)
        {
            // try getting data from cache
            var data = await fetchFromCache();
            if (data is not null)
            {
                return data;
            }

            // Ensure only one thread queries DB per key
            var cacheLock = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

            await cacheLock.WaitAsync();
            try
            {
                // Double-check if another thread has updated the cache
                data = await fetchFromCache();
                if (data is not null)
                {
                    return data;
                }

                // Fetch data from DB and update cache
                data = await fetchFromDb();
                await saveToCache(data);

                return data;
            }
            finally
            {
                cacheLock.Release();
                _locks.TryRemove(key, out _); // Remove unused lock to free memory
            }
        }
        public async Task<bool> CheckAndFetchAsync(
                string key,
                string member,
                Func<Task<int>> fetchFromCache, // Should return 0, 1, or 2
                Func<Task<List<string>>> fetchFromDb, // Fetch from DB
                Func<List<string>, Task> saveToCache // Save to cache
            )
        {
            // Check from cache
            int cacheResult = await fetchFromCache();

            if (cacheResult == 1)
            {
                return true; // User exists, expiry extended
            }
            if (cacheResult == 0)
            {
                return false; // List exists, but user is not in it
            }

            // If expired (cacheResult == 2), ensure only one thread fetches from DB
            var cacheLock = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

            await cacheLock.WaitAsync();
            try
            {
                // Double-check cache after acquiring lock
                cacheResult = await fetchFromCache();
                if (cacheResult == 1) return true;
                if (cacheResult == 0) return false;

                // Fetch fresh data from DB
                var dbResult = await fetchFromDb();

                // Save new data to cache
                await saveToCache(dbResult);

                // Check if user exists in the refreshed list
                return dbResult.Contains(member);
            }
            finally
            {
                cacheLock.Release();
                _locks.TryRemove(key, out _); // Cleanup lock
            }
        }
    }
}