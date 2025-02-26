using iChat.Data.Entities.Users;
using System.Threading.Tasks;

namespace iChat.BackEnd.Services.Users.Auth
{
    public interface IJwtService
    {
        Task<string> GenerateAccessToken(AppUser user);
        string GenerateRefreshToken();
    }
}
