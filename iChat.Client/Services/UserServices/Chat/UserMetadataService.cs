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
    public class UserMetadataService
    {
        private readonly ConcurrentDictionary<long, UserMetadataReact> _cache = new();
        private readonly ConcurrentQueue<long> _pendingIds = new();
        private readonly HashSet<long> _inFlight = new();
        private readonly System.Timers.Timer _timer;
        public event Action<List<UserMetadataReact>>? _onMetadataUpdated;
        JwtAuthHandler _https;
        public IEnumerable<UserMetadataReact> GetAll()
        {
                       return _cache.Values;
        }
        public UserMetadataService(JwtAuthHandler jwtAuthHandler)
        {
            _https = jwtAuthHandler;
            _timer = new System.Timers.Timer(500);
            _timer.Elapsed += async (_, _) => await ProcessQueueAsync();
            _timer.Start();
        }

        public Task<UserMetadataReact> GetUserByIdAsync(long userId)
        {
            if (_cache.TryGetValue(userId, out var cached))
                return Task.FromResult(cached);
            var placeholder = new UserMetadataReact(userId, $"{userId}", "https://cdn.discordapp.com/embed/avatars/1.png");
            _cache[userId] = placeholder;

            lock (_inFlight)
            {
                if (_inFlight.Add(userId))
                    _pendingIds.Enqueue(userId);
            }

            return Task.FromResult(placeholder);
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
                if (string.IsNullOrEmpty(fetched.AvatarUrl))
                    fetched.AvatarUrl = $"https://cdn.discordapp.com/embed/avatars/0.png";
                if (_cache.TryGetValue(fetched.UserId, out var existing))
                {
                    existing.DisplayName = fetched.DisplayName;
                    existing.AvatarUrl = fetched.AvatarUrl;
                    updatedList.Add(existing);
                }
                else
                {
                    _cache[fetched.UserId] = fetched;
                    updatedList.Add(fetched);
                }
            }
            _onMetadataUpdated?.Invoke(updatedList);
        }
        private async Task<List<UserMetadataReact>> FetchMetadataBatchAsync(List<long> ids)
        {
            if (ids.Count < 5)
            {
                var tasks = ids.Select(async id =>
                {
                    try
                    {
                        using var response = await _https.SendAuthAsync(new HttpRequestMessage(HttpMethod.Get, $"/api/users/GetUserById?userId={Uri.EscapeDataString(id.ToString())}"));
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
                    Content = new StringContent(JsonSerializer.Serialize(ids), Encoding.UTF8, "application/json")
                };
                var response = await _https.SendAuthAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadFromJsonAsync<List<UserMetadataReact>>();
                    return content ?? new List<UserMetadataReact>();
                }
                else
                {
                    return new List<UserMetadataReact>();
                }
            }
        }

        public void Dispose() => _timer.Dispose();
        public ValueTask DisposeAsync() { _timer.Dispose(); return ValueTask.CompletedTask; }
    }
}