using iChat.DTOs.Users.Auth;

namespace iChat.BackEnd.Services.Users.Auth
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterRequest request);
        Task<bool> LoginAsync(LoginRequest request);
        Task LogoutAsync();
    }
}
