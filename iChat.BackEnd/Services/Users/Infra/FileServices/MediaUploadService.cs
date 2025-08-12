using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.DB;
using iChat.Data.EF;
using iChat.Data.Entities.Users.Messages;
using iChat.DTOs.Collections;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace iChat.BackEnd.Services.Users.Infra.FileServices
{
    public class MediaUploadService : IMediaUploadService, IDisposable
    {
        private readonly IWebHostEnvironment _env;
        private readonly iChatDbContext _dbContext;
        private readonly List<PendingFile> _pendingFiles = new();
        private bool _committed;
        private record PendingFile(MediaFile Entity, string TempPath, string FinalPath);
        public MediaUploadService(IWebHostEnvironment env, iChatDbContext dbContext)
        {
            _env = env;
            _dbContext = dbContext;
        }
        public async Task<MediaFile?> SaveImageWithHashDedupAsync(IFormFile file, UserId uploaderUserId, ImageUploadProfile profile)
        {
            var hash = await ComputeSha256Async(file.OpenReadStream());

            if (await _dbContext.MediaFiles.AnyAsync(m => m.Hash == hash && !m.IsDeleted))
                return null;

            return await SaveProcessedImageAsync(file, uploaderUserId, profile, $"{hash}.webp", hash);
        }
        public async Task<MediaFile> SaveImageWithProvidedIdAsync(IFormFile file, UserId uploaderUserId, ImageUploadProfile profile, string fileName)
        {
            var hash = await ComputeSha256Async(file.OpenReadStream());
            return await SaveProcessedImageAsync(file, uploaderUserId, profile, fileName, hash);
        }
        private async Task<MediaFile> SaveProcessedImageAsync(IFormFile file, UserId uploaderUserId, ImageUploadProfile profile, string fileName, string hash)
        {
            using var image = await Image.LoadAsync(file.OpenReadStream());
            profile.Transform(image);
            var dirPath = Path.Combine(_env.WebRootPath, "uploads", profile.Folder);
            Directory.CreateDirectory(dirPath);

            var finalPath = Path.Combine(dirPath, fileName);
            var tempPath = $"{finalPath}.temp";
            var url = $"/api/uploads/{profile.Folder}/{fileName}";
            await image.SaveAsync(tempPath, new WebpEncoder { Quality = profile.WebpQuality });
            var mediaFile = new MediaFile
            {
                Hash = hash,
                Url = url,
                ContentType = "image/webp",
                Width = image.Width,
                Height = image.Height,
                SizeBytes = (int)new FileInfo(tempPath).Length,
                UploadedAt = DateTime.UtcNow,
                UploaderUserId = uploaderUserId.Value
            };

            _pendingFiles.Add(new PendingFile(mediaFile, tempPath, finalPath));
            return mediaFile;
        }
        public async Task CommitAsync()
        {
            if (!_pendingFiles.Any())
                return;

            ValidateTargetFilesNotExist();
            await AtomicFileMove();
            await SaveToDatabase();

            _committed = true;
            _pendingFiles.Clear();
        }
        public Task RollbackAsync()
        {
            CleanupTempFiles();
            _committed = true;
            _pendingFiles.Clear();
            return Task.CompletedTask;
        }
        private void ValidateTargetFilesNotExist()
        {
            var existingFile = _pendingFiles.FirstOrDefault(f => File.Exists(f.FinalPath));
            if (existingFile != null)
                throw new IOException($"Target file already exists: {existingFile.FinalPath}");
        }
        private async Task AtomicFileMove()
        {
            var movedFiles = new List<PendingFile>();
            try
            {
                foreach (var file in _pendingFiles)
                {
                    if (!File.Exists(file.TempPath))
                        throw new FileNotFoundException("Temp file missing during commit", file.TempPath);
                    File.Move(file.TempPath, file.FinalPath);
                    movedFiles.Add(file);
                }
            }
            catch
            {
                RollbackMovedFiles(movedFiles);
                throw new IOException("Failed to finalize staged files during commit.");
            }
        }
        private async Task SaveToDatabase()
        {
            try
            {
                _dbContext.MediaFiles.AddRange(_pendingFiles.Select(f => f.Entity));
                await _dbContext.SaveChangesAsync();
            }
            catch
            {
                RollbackMovedFiles(_pendingFiles);
                throw new IOException("Database save failed during commit; staged files were reverted.");
            }
        }
        private void RollbackMovedFiles(IEnumerable<PendingFile> files)
        {
            foreach (var file in files)
            {
                try
                {
                    if (File.Exists(file.FinalPath))
                        File.Move(file.FinalPath, file.TempPath);
                }
                catch { }
            }
        }

        private void CleanupTempFiles()
        {
            foreach (var file in _pendingFiles)
            {
                try { File.Delete(file.TempPath); }
                catch { }
            }
        }
        private static async Task<string> ComputeSha256Async(Stream stream)
        {
            stream.Position = 0;
            var hash = await SHA256.HashDataAsync(stream);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
        public void Dispose()
        {
            if (!_committed && _pendingFiles.Any())
                CleanupTempFiles();
        }
    }
}