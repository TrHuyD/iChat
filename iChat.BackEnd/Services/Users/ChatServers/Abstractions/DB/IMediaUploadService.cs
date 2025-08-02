using iChat.Data.Entities.Users.Messages;
using iChat.DTOs.Collections;
using System.Threading.Tasks;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions.DB
{
    public interface IMediaUploadService
    {
        Task<MediaFile> SaveAvatarAsync(IFormFile file, UserId uploaderUserId);
        Task<MediaFile> SaveImageAsync(IFormFile file, UserId uploaderUserId);
    }
}
