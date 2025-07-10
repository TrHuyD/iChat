using iChat.Client.Services.Auth;
using iChat.DTOs.Users.Messages;
using System.Net.Http.Json;

namespace iChat.Client.Services.UserServices
{
    public class InviteService
    {
        private readonly JwtAuthHandler _http;
        private readonly ILogger<InviteService> _logger;

        public InviteService(JwtAuthHandler http, ILogger<InviteService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<ChatServerMetadata?> GetServerFromInvite(string inviteId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/Chat/InviteLink/{inviteId}");
                var response = await _http.SendAuthAsync(request);
                if(!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to parse invite link: {StatusCode}", response.StatusCode);
                    return null;
                }
                var result = await response.Content.ReadFromJsonAsync<ChatServerMetadata>();
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning("Failed to parse invite link: {Message}", ex.Message);
                return null;
            }
        }
        public async Task<long?> UseInvite(string inviteId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"api/Chat/InviteLink/{inviteId}");
                var response = await _http.SendAuthAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to join server via invite: {Response}", error);
                    return null;
                }
                var result = await response.Content.ReadFromJsonAsync<InviteJoinResult>();
                return result?.ServerId;

            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Error joining server with invite: {Message}", ex.Message);
                return null;
            }
        }

        public async Task<string?> CreateInvite(string serverId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/Chat/{serverId}/InviteLink");
                var response = await _http.SendAuthAsync(request);
                if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Unauthorized access while creating invite link for server {ServerId}", serverId);
                    return null;
                }
                return "https://localhost:7156/inv/"+ await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning("Failed to create invite link: {Message}", ex.Message);
                return null;
            }
        }

        private class InviteJoinResult
        {
            public long ServerId { get; set; }
        }
    }

}