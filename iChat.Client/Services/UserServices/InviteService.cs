using iChat.Client.Services.Auth;
using iChat.DTOs.ChatServerDatas;
using System.Net.Http.Json;

namespace iChat.Client.Services.UserServices
{
    public class InviteService
    {
        private readonly JwtAuthHandler _http;
        private readonly ILogger<InviteService> _logger;
        private readonly Dictionary<string, string> _inviteCache = new();
        private readonly ConfigService _configService;
        public InviteService(JwtAuthHandler http,ConfigService configService, ILogger<InviteService> logger)
        {
            _configService = configService;
            _http = http;
            _logger = logger;
        }

        public async Task<ChatServerData?> GetServerFromInvite(string inviteId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/Chat/InviteLink/{inviteId}");
                var response = await _http.SendAuthAsync(request);
                if(!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Failed to parse invite link: {StatusCode}", response.StatusCode);
                    _logger.LogWarning("Failed to parse invite link: {StatusCode}", response.StatusCode);
                    return null;
                }
                var result = await response.Content.ReadFromJsonAsync<ChatServerData>();
                return result;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("Failed to parse invite link: {Message}", ex.Message);
                _logger.LogWarning("Failed to parse invite link: {Message}", ex.Message);
                return null;
            }
        }
        public async Task UseInvite(string inviteId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"/api/Chat/InviteLink/{inviteId}");
                var response = await _http.SendAuthAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to join server via invite: {Response}", error);
                }
              //  return result?.Id;

            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Error joining server with invite: {Message}", ex.Message);
            }
        }

        public async Task<string?> CreateInvite(string serverId)
        {
            try
            {
                var value = "";
                if (!_inviteCache.TryGetValue(serverId, out value))
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, $"/api/Chat/{serverId}/InviteLink");
                    var response = await _http.SendAuthAsync(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        _logger.LogWarning("Unauthorized access while creating invite link for server {ServerId}", serverId);
                        return null;
                    }
                    value = await response.Content.ReadAsStringAsync();
                    _inviteCache[serverId] = value;
                }
                return _configService.baseurl+"/inv/" + value;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning("Failed to create invite link: {Message}", ex.Message);
                return null;
            }
        }

        private class InviteJoinResult
        {
            public string ServerId { get; set; }
        }
    }

}