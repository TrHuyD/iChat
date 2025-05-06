using System.Net.Http.Json;

namespace iChat.Client.Services
{
    public class UserStateService
    {
        private readonly HttpClient _http;
        private UserProfileDto? _user;

        public UserStateService(HttpClient http)
        {
            _http = http;
        }

        public async Task<UserProfileDto?> GetUserAsync()
        {
            if (_user != null)
                return _user;

            try
            {
                _user = await _http.GetFromJsonAsync<UserProfileDto>("/api/user/profile");
            }
            catch
            {
                
            }

            return _user;
        }

        public bool IsAuthenticated => _user != null;

        public void Clear()
        {
            _user = null;
        }
    }

}
