using iChat.Data.Entities.Users.Messages;
using iChat.DTOs.Collections;

namespace iChat.BackEnd.Services.Users.Infra.FileServices
{
    public interface IGenericMediaUploadService
    {
        Task<MediaFile> SaveAvatarAsync(IFormFile file, UserId uploaderUserId);
        Task<MediaFile> SaveImageAsync(IFormFile file, UserId uploaderUserId);
    }
}
