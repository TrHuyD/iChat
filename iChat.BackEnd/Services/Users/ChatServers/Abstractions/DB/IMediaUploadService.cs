using iChat.Data.Entities.Users.Messages;
using System.Threading.Tasks;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions.DB
{
    public interface IMediaUploadService
    {
        Task<MediaFile> SaveAvatarAsync(IFormFile file, long? uploaderUserId);
        Task<MediaFile> SaveImageAsync(IFormFile file, long? uploaderUserId);
    }
}
