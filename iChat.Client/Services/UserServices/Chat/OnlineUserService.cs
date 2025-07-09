using iChat.Client.Services.Auth;

namespace iChat.Client.Services.UserServices.Chat
{
    public class OnlineUserService
    {
        private readonly JwtAuthHandler _authHandler;
        public OnlineUserService(JwtAuthHandler https)
        {
            _authHandler = https;
        }
        //public async Task<List<string>> GetOnlineUsersAsync()
        //{
        //    var response = await _authHandler.SendAuthAsync(new HttpRequestMessage(HttpMethod.Get, "/api/users/online"), false);
        //    if (response.IsSuccessStatusCode)
        //    {
        //        return await response.Content.ReadFromJsonAsync<List<string>>() ?? new List<string>();
        //    }
        //    return new List<string>();
        //}
    }
}
