using iChat.BackEnd.Services.Users.ChatServers.Abstractions.DB;
using iChat.Data.EF;
using iChat.Data.Entities.Users.Messages;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.Formats.Webp;
using System;
using System.Security.Cryptography;

namespace iChat.BackEnd.Services.Users.Infra.FileServices
{
    public class MediaUploadService : IMediaUploadService
    {
        private readonly IWebHostEnvironment _env;
        private readonly iChatDbContext _dbContext;

        public MediaUploadService(IWebHostEnvironment env, iChatDbContext dbContext)
        {
            _env = env;
            _dbContext = dbContext;
        }

        public async Task<MediaFile> SaveAvatarAsync(IFormFile file, long? uploaderUserId)
        {
            return await SaveTransformedImageAsync(file, uploaderUserId,
                folder: "avatars",
                transform: img => img.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(128, 128),
                    Mode = ResizeMode.Crop
                })),
                webpQuality: 75);
        }

        public async Task<MediaFile> SaveImageAsync(IFormFile file, long? uploaderUserId)
        {
            return await SaveTransformedImageAsync(file, uploaderUserId,
                folder: "images",
                transform: img => { /* No resize, just optimize */ },
                webpQuality: 85);
        }
        private async Task<MediaFile> SaveTransformedImageAsync(
            IFormFile file,
            long? uploaderUserId,
            string folder,
            Action<Image> transform,
            int webpQuality)
        {
            using var originalStream = file.OpenReadStream();
            string hash = await ComputeSha256Async(originalStream);
            var existing = await _dbContext.MediaFiles
                .FirstOrDefaultAsync(m => m.Hash == hash && !m.IsDeleted);
            if (existing != null)
                return existing;
            originalStream.Position = 0;
            using var image = await Image.LoadAsync(originalStream);
            // Apply transformations (resize, crop, etc.)
            transform(image);

            int width = image.Width;
            int height = image.Height;

            string fileName = $"{hash}.webp";
            string dirPath = Path.Combine(_env.WebRootPath, "uploads", folder);
            string savePath = Path.Combine(dirPath, fileName);
            string url = $"/api/uploads/{folder}/{fileName}";
            Directory.CreateDirectory(dirPath); 
            // Save as WebP
            var encoder = new WebpEncoder { Quality = webpQuality };
            await image.SaveAsync(savePath, encoder);
            var fileInfo = new FileInfo(savePath);
            var mediaFile = new MediaFile
            {
                Hash = hash,
                Url = url,
                ContentType = "image/webp",
                Width = width,
                Height = height,
                SizeBytes = (int)fileInfo.Length,
                UploadedAt = DateTime.UtcNow,
                UploaderUserId = uploaderUserId
            };
            _dbContext.MediaFiles.Add(mediaFile);
            await _dbContext.SaveChangesAsync();
            return mediaFile;
        }
        private async Task<string> ComputeSha256Async(Stream stream)
        {
            stream.Position = 0;
            using var sha256 = SHA256.Create();
            var hash = await sha256.ComputeHashAsync(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

}