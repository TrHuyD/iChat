using iChat.DTOs.Collections;
using iChat.DTOs.Users;
using iChat.ViewModels.Users;
using Neo4j.Driver;
using System.Linq.Dynamic.Core;

namespace iChat.BackEnd.Services.Users.Auth
{
    public interface IUserService
    {

        [Obsolete("Use Get usermetadata instead.")]
        Task<UserProfileDto?> GetUserProfileAsync(UserId userId);
        Task<UserCompleteDto?> GetUserCompleteInfoAsync(UserId userId);
        Task<List<UserMetadata>> GetUserMetadataBatchAsync(List<string> userIds);
        Task<UserMetadata> GetUserMetadataAsync(UserId userId);
        Task<UserMetadata> EditUserNickNameAsync(UserId userId, string newNickName);
        Task<UserMetadata> EditAvatarAsync(UserId userId, string avatarUrl);
        Task<UserMetadata> EditNameAndAvatarAsync(UserId userId,string newNickName, string avatarUrl);

    }
}
