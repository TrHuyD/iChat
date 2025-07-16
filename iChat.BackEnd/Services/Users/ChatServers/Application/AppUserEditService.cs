using iChat.BackEnd.Models.Helpers;
using iChat.BackEnd.Services.Users.Auth;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.DB;
using iChat.DTOs.Users;

namespace iChat.BackEnd.Services.Users.ChatServers.Application
{
    public class AppUserEditService
    {
        private readonly IUserMetaDataCacheService _userMetaDataCacheService;
        private readonly Lazy<IUserService> _userService;
        private readonly Lazy<IMediaUploadService> uploadService;

        public AppUserEditService(IUserMetaDataCacheService userMetaDataCacheService, Lazy<IUserService> userService,Lazy<IMediaUploadService> mediaUploadService)
        {
            _userMetaDataCacheService = userMetaDataCacheService;
            uploadService = mediaUploadService;
            _userService = userService;
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
        public async Task<UserMetadata> UpdateUserAvatar(string UserId, IFormFile file)
        {
            var userId = long.Parse(UserId);
            var _result = (await uploadService.Value.SaveAvatarAsync(file, userId));
            var result = _result.ToDto();
            return await UpdateUserAvatar(UserId, result.Url);
        }
        public async Task<UserMetadata> UpdateUserNameAndAvatar(string UserId, string UserName, IFormFile file)
        {
            var userId = long.Parse(UserId);
            var _result = (await uploadService.Value.SaveAvatarAsync(file, userId));
            var result = _result.ToDto();
            var metadata = await _userService.Value.EditNameAndAvatarAsync(UserId, UserName, result.Url);
            _ = _userMetaDataCacheService.SetAsync(metadata);
            return metadata;
        }
    }
    }
