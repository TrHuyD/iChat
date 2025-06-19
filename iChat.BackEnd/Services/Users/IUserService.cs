using iChat.DTOs.Users;
using iChat.ViewModels.Users;
using Neo4j.Driver;
using System.Linq.Dynamic.Core;

namespace iChat.BackEnd.Services.Users.Auth
{
    public interface IUserService
    {
   
        Task<UserProfileDto?> GetUserProfileAsync(long userId);
        Task<UserCompleteDto?> GetUserCompleteInfoAsync(long userId);
    }
}
