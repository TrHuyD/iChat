using iChat.BackEnd.Services.Users.Auth;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.DTOs.Users;

namespace iChat.BackEnd.Services.Users.ChatServers.Application
{
    public class AppUserService
    {
        private readonly IUserMetaDataCacheService _userMetaDataCacheService;
        private readonly Lazy<IUserService> _userService;
        public AppUserService(IUserMetaDataCacheService userMetaDataCacheService, Lazy<IUserService> userService)
        {
            _userMetaDataCacheService = userMetaDataCacheService;
            _userService = userService;
        }
        public async Task<UserMetadata> GetUserMetadataAsync(string userId)
        {
            var cachedMetadata = await _userMetaDataCacheService.GetAsync(userId);
            if (cachedMetadata != null)
            {
                return cachedMetadata;
            }
            var usermetadata = await _userService.Value.GetUserMetadataAsync(userId);
            _ = _userMetaDataCacheService.SetAsync(usermetadata);
            return usermetadata;
        }
        //public async Task<UserMetadata> EditUserMetaData(string userId)
        //{

        //}
        public async Task<List<UserMetadata>> GetUserMetadataBatchAsync(List<string> userIds)
        {
            if (userIds.Count > 50)
                throw new ArgumentException("Batch size exceeds the limit of 50 users.");
            var cachedMetadata = await _userMetaDataCacheService.GetManyAsync(userIds);
            var missingUserIds = cachedMetadata.missing;
            var metadata = cachedMetadata.dic;
            if (missingUserIds.Count > 0)
            {
                var userMetadataList = await _userService.Value.GetUserMetadataBatchAsync(missingUserIds);
                foreach (var userMetadata in userMetadataList)
                {
                    metadata[userMetadata.UserId] = userMetadata;
                    _ = _userMetaDataCacheService.SetAsync(userMetadata);
                }
            }
            return userIds.Select(id => metadata.ContainsKey(id) ? metadata[id] : null).Where(x => x != null).ToList();
        }
        public async Task<UserMetadata> UpdateUserName(string UserId, string UserName)
        {
            var userMetadata = await _userService.Value.EditUserNickNameAsync(UserId, UserName);
            _ = _userMetaDataCacheService.SetAsync(userMetadata);
            return userMetadata;

        }
        public async Task<UserMetadata> UpdateUserAvatar(string UserId, string AvatarUrl)
        {
            var userMetadata = await _userService.Value.EditAvatarAsync(UserId, AvatarUrl);
            _ = _userMetaDataCacheService.SetAsync(userMetadata);
            return userMetadata;
        }
    }
}
