using iChat.Client.Services.Auth;
using iChat.DTOs.Users;
using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Timers;
using static System.Net.WebRequestMethods;
namespace iChat.Client.Services.UserServices.Chat
{
    public class UserMetadataService
    {
        private readonly ConcurrentDictionary<string, UserMetadata> _cache = new();
        private readonly ConcurrentQueue<string> _pendingIds = new();
        private readonly HashSet<string> _inFlight = new(StringComparer.Ordinal);
        private readonly System.Timers.Timer _timer;
        public event Action<List<UserMetadata>>? _onMetadataUpdated;
        JwtAuthHandler _https;
        public UserMetadataService(JwtAuthHandler jwtAuthHandler)
        {
            _https = jwtAuthHandler;
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
            return Task.FromResult(new UserMetadata(userId, $"User {userId}", "https://cdn.discordapp.com/embed/avatars/1.png"));
        }
        private async Task ProcessQueueAsync()
        {
            var batch = new List<string>();
            while (batch.Count < 10 && _pendingIds.TryDequeue(out var uid))
                batch.Add(uid);
            if (batch.Count == 0) return;
            var results = await FetchMetadataBatchAsync(batch);
            foreach (var u in results)
            {
   
                _cache[u.UserId] = u;
            }
            lock (_inFlight)
            {
               
                foreach (var uid in batch)
                {

                    _inFlight.Remove(uid);
               
                }
               
            }
            _onMetadataUpdated?.Invoke(results);
        }

        private async Task<List<UserMetadata>> FetchMetadataBatchAsync(List<string> ids)
        {
            if (ids.Count < 5) 
            {
                var tasks = ids.Select(async id =>
                {
                    try
                    {
                        using var response = await _https.SendAuthAsync( new HttpRequestMessage(HttpMethod.Get, $"/api/users/GetUserById?userId={Uri.EscapeDataString(id)}") );
                        if (!response.IsSuccessStatusCode)
                            return null;
                        var json = await response.Content.ReadFromJsonAsync<UserMetadata>();
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
                    var content = await response.Content.ReadFromJsonAsync<List<UserMetadata>>();
                    return content;
                }
                else
                {//do nothing for now
                    return new List<UserMetadata>();
                }
            }

        }
        public void Dispose() => _timer.Dispose();
        public ValueTask DisposeAsync() { _timer.Dispose(); return ValueTask.CompletedTask; }

       
    }

    

}
