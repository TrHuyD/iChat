using iChat.BackEnd.Models.Helpers;
using iChat.BackEnd.Services.Users.Auth;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.DB;
using iChat.BackEnd.Services.Users.Infra.FileServices;
using iChat.DTOs.Collections;
using iChat.DTOs.Users;

namespace iChat.BackEnd.Services.Users.ChatServers.Application
{
    public class AppUserEditService
    {
        private readonly IUserMetaDataCacheService _userMetaDataCacheService;
        private readonly Lazy<IUserService> _userService;
        IGenericMediaUploadService uploadService;

        public AppUserEditService(IUserMetaDataCacheService userMetaDataCacheService, Lazy<IUserService> userService, IGenericMediaUploadService mediaUploadService)
        {
            _userMetaDataCacheService = userMetaDataCacheService;
            uploadService = mediaUploadService;
            _userService = userService;
        }
        public async Task<UserMetadata> UpdateUserName(UserId UserId, string UserName)
        {
            var userMetadata = await _userService.Value.EditUserNickNameAsync(UserId, UserName);
            _ = _userMetaDataCacheService.SetAsync(userMetadata);
            return userMetadata;

        }
        public async Task<UserMetadata> UpdateUserAvatar(UserId userId, string AvatarUrl)
        {
            var userMetadata = await _userService.Value.EditAvatarAsync(userId, AvatarUrl);
            _ = _userMetaDataCacheService.SetAsync(userMetadata);
            return userMetadata;
        }
        public async Task<UserMetadata> UpdateUserAvatar(UserId userId, IFormFile file)
        {
            var _result = (await uploadService.SaveAvatarAsync(file, userId));
            var result = _result.ToDto();
            return await UpdateUserAvatar(userId, result.Url);
        }
        public async Task<UserMetadata> UpdateUserNameAndAvatar(UserId userId, string UserName, IFormFile file)
        {
            var _result = (await uploadService.SaveAvatarAsync(file, userId));
            var result = _result.ToDto();
            var metadata = await _userService.Value.EditNameAndAvatarAsync(userId, UserName, result.Url);
            _ = _userMetaDataCacheService.SetAsync(metadata);
            return metadata;
        }
    }
    }
