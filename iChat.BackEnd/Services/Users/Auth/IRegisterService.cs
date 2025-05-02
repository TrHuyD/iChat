using iChat.DTOs.Shared;
using iChat.DTOs.Users.Auth;

namespace iChat.BackEnd.Services.Users.Auth
{
    public interface IRegisterService
    {
        public  Task<OperationResult> CreateUserAsync(RegisterRequest rq);
    }
}
