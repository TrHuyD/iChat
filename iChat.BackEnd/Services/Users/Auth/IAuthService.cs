using iChat.DTOs.Users.Auth;

namespace iChat.BackEnd.Services.Users.Auth
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
    }
}
