using iChat.App.Models.User.Auth;
using iChat.DTOs.Users.Auth;

namespace iChat.App.Services.User
{
    public interface IAuthService
    {
        public Task<AuthResult> LoginAsync(LoginRequest request);
    }
}
