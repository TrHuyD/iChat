using iChat.DTOs.Shared;
using iChat.DTOs.Users.Auth;

namespace iChat.BackEnd.Services.Users.Auth
{
    public interface ILoginService
    {
        public Task<OperationResult> LoginAsync(LoginRequest request, HttpContext context);
    }
}
