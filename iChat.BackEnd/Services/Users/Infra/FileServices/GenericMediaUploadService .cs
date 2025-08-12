using iChat.BackEnd.Services.Users.ChatServers.Abstractions.DB;
using iChat.Data.Entities.Users.Messages;
using iChat.DTOs.Collections;

namespace iChat.BackEnd.Services.Users.Infra.FileServices
{
    public class GenericMediaUploadService : IGenericMediaUploadService
    {
        private readonly IMediaUploadService _mediaUploadService;

        public GenericMediaUploadService(IMediaUploadService mediaUploadService)
        {
            _mediaUploadService = mediaUploadService;
        }
        public Task<MediaFile> SaveAvatarAsync(IFormFile file, UserId uploaderUserId) =>
            _mediaUploadService.SaveImageWithHashDedupAsync(file, uploaderUserId, ImageUploadProfiles.Avatar);
        public Task<MediaFile> SaveImageAsync(IFormFile file, UserId uploaderUserId) =>
            _mediaUploadService.SaveImageWithHashDedupAsync(file, uploaderUserId, ImageUploadProfiles.GeneralImage);
    }

}
