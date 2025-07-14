using iChat.DTOs.Users;
using iChat.ViewModels.Users;
using Neo4j.Driver;
using System.Linq.Dynamic.Core;

namespace iChat.BackEnd.Services.Users.Auth
{
    public interface IUserService
    {

        [Obsolete("Use Get usermetadata instead.")]
        Task<UserProfileDto?> GetUserProfileAsync(string userId);
        Task<UserCompleteDto?> GetUserCompleteInfoAsync(string userId);
        Task<List<UserMetadata>> GetUserMetadataBatchAsync(List<string> userIds);
        Task<UserMetadata> GetUserMetadataAsync(string userId);
        Task<UserMetadata> EditUserNickNameAsync(string userId, string newNickName);
        Task<UserMetadata> EditAvatarAsync(string userId, string avatarUrl);
    }
}
