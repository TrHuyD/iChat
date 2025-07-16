using iChat.Data.Entities.Users.Messages;
using iChat.DTOs.Users;

namespace iChat.BackEnd.Models.Helpers
{
    public static class MediaFileExtensions
    {
        public static MediaFileDto? ToDto(this MediaFile mediaFile)
        {
            if (mediaFile == null) return null;

            return new MediaFileDto
            {
                Id = mediaFile.Id,
                Hash = mediaFile.Hash,
                Url = mediaFile.Url,
                ContentType = mediaFile.ContentType,
                Width = mediaFile.Width,
                Height = mediaFile.Height,
                SizeBytes = mediaFile.SizeBytes,
                UploadedAt = mediaFile.UploadedAt,
                IsDeleted = mediaFile.IsDeleted
            };
        }
    }
}
