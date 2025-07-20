using iChat.Client.DTOs.Chat;
using iChat.Client.Services.Auth;
using iChat.DTOs.Users;
using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Timers;

namespace iChat.Client.Services.UserServices.Chat
{
    public class UserMetadataService : IDisposable, IAsyncDisposable
    {
        private readonly ConcurrentDictionary<long, UserMetadataReact> _cache = new();
        private readonly ConcurrentQueue<long> _pendingIds = new();
        private readonly HashSet<long> _inFlight = new();
        private readonly System.Timers.Timer _timer;
        private  long userId;
        public event Action? OnMetadataUpdated;
        //public event Action? OnUserProfileUpdated;
        private readonly JwtAuthHandler _https;
        public IEnumerable<UserMetadataReact> GetAll() => _cache.Values;
        public Action<long> OnMetadataChangeSpecifc;
        public void RegisterOnSpecificMetadataChange(Action<long> action)=> OnMetadataChangeSpecifc += action;
        public UserMetadataService(JwtAuthHandler jwtAuthHandler)
        {
            _https = jwtAuthHandler;
            _timer = new System.Timers.Timer(500);
            _timer.Elapsed += async (_, _) => await ProcessQueueAsync();
            _timer.Start();
        }
        public UserMetadataReact GetUserByIdAsync(long userId)
        {
            if (_cache.TryGetValue(userId, out var cached))
                return cached;

            var placeholder = new UserMetadataReact(userId, $"{userId}", "https://cdn.discordapp.com/embed/avatars/1.png",0);
            _cache[userId] = placeholder;

            QueueUserIdForUpdate(userId);
            return placeholder;
        }
        public void SyncMetadataVersion(long userId,long  version)
        {
            if (_cache.TryGetValue(userId, out var existing))
            {
                if (existing.Version == 0)
                    return;
                if (existing.Version != version)
                    QueueUserIdForUpdate(userId);
            }
        }
        public void QueueUserIdForUpdate(long userId)
        {
            lock (_inFlight)
            {
                if (_inFlight.Add(userId))
                    _pendingIds.Enqueue(userId);
            }
        }
        public void SetUserProfile(UserMetadata metadata)
        {
            if (metadata == null) return;
            var userId = long.Parse(metadata.UserId);
            this.userId = userId;
            metadata.AvatarUrl ??= $"https://cdn.discordapp.com/embed/avatars/0.png";
            if (_cache.TryGetValue(userId, out var existing))
            {
                existing.DisplayName = metadata.DisplayName;
                existing.AvatarUrl=metadata.AvatarUrl;
            }
            else
            {
                var userMetadata = new UserMetadataReact(userId, metadata.DisplayName, metadata.AvatarUrl,long.Parse(metadata.Version));
            }
            OnMetadataUpdated?.Invoke();
        }
        public void AddOrUpdateMetadata(UserMetadataReact metadata)
        {
            if (metadata == null) return;
            if (_cache.TryGetValue(metadata.UserId, out var existing))
            {
                existing.DisplayName = metadata.DisplayName;
                existing.AvatarUrl = metadata.AvatarUrl;
          
            }
            else
            {
                metadata.AvatarUrl ??= $"https://cdn.discordapp.com/embed/avatars/0.png";
                _cache[metadata.UserId] = metadata;
            }
            OnMetadataChangeSpecifc?.Invoke(metadata.UserId);
            OnMetadataUpdated?.Invoke();

        }
        private async Task ProcessQueueAsync()
        {
            var batch = new List<long>();
            while (batch.Count < 10 && _pendingIds.TryDequeue(out var uid))
                batch.Add(uid);

            if (batch.Count == 0) return;

            var results = await FetchMetadataBatchAsync(batch);
            var updatedList = new List<UserMetadataReact>();

            lock (_inFlight)
            {
                foreach (var uid in batch)
                    _inFlight.Remove(uid);
            }

            foreach (var fetched in results)
            {
                AddOrUpdateMetadata(fetched);
            }
        }
        private async Task<List<UserMetadataReact>> FetchMetadataBatchAsync(List<long> ids)
        {
            if (ids.Count < 5)
            {
                var tasks = ids.Select(async id =>
                {
                    try
                    {
                        using var response = await _https.SendAuthAsync(
                            new HttpRequestMessage(HttpMethod.Get, $"/api/users/GetUserById?userId={Uri.EscapeDataString(id.ToString())}"));

                        if (!response.IsSuccessStatusCode)
                            return null;

                        var json = await response.Content.ReadFromJsonAsync<UserMetadataReact>();
                        return json;
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                });

                var results = await Task.WhenAll(tasks);
                return results.Where(r => r != null).ToList()!;
            }
            else
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "/api/users/GetUsersByIds")
                {
                    Content = new StringContent(JsonSerializer.Serialize(ids.Select(i=>i.ToString()).ToList()), Encoding.UTF8, "application/json")
                };

                var response = await _https.SendAuthAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadFromJsonAsync<List<UserMetadataReact>>();
                    return content ?? new List<UserMetadataReact>();
                }

                return new List<UserMetadataReact>();
            }
        }
        public void Dispose() => _timer.Dispose();
        public ValueTask DisposeAsync()
        {
            _timer.Dispose();
            return ValueTask.CompletedTask;
        }

        public List<UserMetadataReact> SearchUsers(string query,int limit=6)
        {
            query = query.Trim().ToLowerInvariant();

            var results = _cache.Values
                .Where(user =>
                    !string.IsNullOrEmpty(user.DisplayName) &&
                    user.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Take(limit)
                .ToList();

            return results;
        }
    }
}