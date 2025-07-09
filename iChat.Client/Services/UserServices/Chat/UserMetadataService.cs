using iChat.DTOs.Users;
using System.Collections.Concurrent;
using System.Timers;
namespace iChat.Client.Services.UserServices.Chat
{
    public class UserMetadataService
    {
        private readonly ConcurrentDictionary<string, UserMetadata> _cache = new();
        private readonly ConcurrentQueue<string> _pendingIds = new();
        private readonly HashSet<string> _inFlight = new(StringComparer.Ordinal);
        private readonly System.Timers.Timer _timer;
        public event Action<List<UserMetadata>>? _onMetadataUpdated;
        public UserMetadataService()
        {
            _timer = new System.Timers.Timer(500);
            _timer.Elapsed += async (_, _) => await ProcessQueueAsync();
            _timer.Start();
        }

        public Task<UserMetadata> GetUserByIdAsync(string userId)
        {
            if (_cache.TryGetValue(userId, out var cached))
                return Task.FromResult(cached);
            lock (_inFlight)
            {
                if (_inFlight.Add(userId))
                    _pendingIds.Enqueue(userId);
            }
            return Task.FromResult(new UserMetadata(userId, $"User {userId}", "https://cdn.discordapp.com/embed/avatars/0.png"));
        }
        private async Task ProcessQueueAsync()
        {
            var batch = new List<string>();
            while (batch.Count < 6 && _pendingIds.TryDequeue(out var uid))
                batch.Add(uid);
            if (batch.Count == 0) return;
            var results = await FetchMetadataBatchAsync(batch);
            foreach (var u in results)
                _cache[u.UserId] = u;
            lock (_inFlight)
            {
                foreach (var uid in batch)
                    _inFlight.Remove(uid);
            }
            _onMetadataUpdated?.Invoke(results);
        }

        private Task<List<UserMetadata>> FetchMetadataBatchAsync(IEnumerable<string> ids)
        {

            return Task.FromResult(ids.Select(id =>new UserMetadata(id, $"User {id}", "https://cdn.discordapp.com/embed/avatars/1.png")).ToList());
        }
        public void Dispose() => _timer.Dispose();
        public ValueTask DisposeAsync() { _timer.Dispose(); return ValueTask.CompletedTask; }

       
    }

    

}
