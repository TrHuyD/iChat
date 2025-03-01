using iChat.App.Models.Helper;
using iChat.App.Models.User.Auth;
using iChat.DTOs.Users.Auth;
using Microsoft.Extensions.Options;

namespace iChat.App.Services.User
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ApiAddressSettings _apiSettings;
        private void SetJwtToken(string token)
        {
           
            var context = _httpContext.HttpContext;
            context?.Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true
            });

        }
        private void SetRefreshTokenToken(string token)
        {
            var context = _httpContext.HttpContext;
            context?.Response.Cookies.Append("refreshToken", token, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true
            });
        }
        public AuthService(HttpClient httpClient, IHttpContextAccessor httpContext ,IOptions<ApiAddressSettings> apisetting)
        {
            _apiSettings = apisetting.Value;
            _httpClient = httpClient;
            _httpContext = httpContext;
        }
        public async Task<AuthResult> LoginAsync(LoginRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("https://localhost:7296/api/User/login", request);
            if (!response.IsSuccessStatusCode)
            {
                return new AuthResult
                { Message="Invalid credentials", Success = false };
            }
            
            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            SetJwtToken(authResponse.AccessToken);
            SetRefreshTokenToken(authResponse.RefreshToken);
            return new AuthResult
            {
                Success = true
            };
        }
            
    }
}
